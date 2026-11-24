import { HttpErrorResponse, httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { PageResult } from '../../../../shared/api/page-result';
import { serverMessage } from '../../../../shared/util/server-message';
import { SocialChallengeDto } from '../../api/games-api-types';
import { SocialChallengeAuthoring } from '../social-challenge-authoring';

const PAGE_SIZE = 20;

@Component({
  imports: [RouterLink],
  selector: 'app-social-challenge-list',
  styleUrl: './social-challenge-list.scss',
  templateUrl: './social-challenge-list.html',
})
export class SocialChallengeList {
  private readonly auth = inject(Auth);
  private readonly socialChallengeAuthoring = inject(SocialChallengeAuthoring);

  protected readonly isAdministrator = computed(
    () => this.auth.user()?.roles.includes('administrator') ?? false,
  );

  protected readonly page = signal(1);

  protected readonly challenges = httpResource<PageResult<SocialChallengeDto>>(
    () => `/api/games/social-challenges?page=${this.page()}&pageSize=${PAGE_SIZE}`,
  );

  protected readonly challengesErrorStatus = computed(() => {
    const failure = this.challenges.error();
    return failure instanceof HttpErrorResponse ? failure.status : null;
  });

  protected readonly pending = signal(false);
  // A failed row action shows under its row; a success shows above the table.
  protected readonly rowError = signal<{ rowId: string; message: string } | null>(null);
  protected readonly success = signal<string | null>(null);

  // Archiving and deleting cannot be undone, so the row first asks for confirmation.
  protected readonly awaitingConfirmation = signal<{
    id: string;
    action: 'archive' | 'delete';
  } | null>(null);

  protected hasNextPage(totalCount: number | undefined): boolean {
    return this.page() * PAGE_SIZE < (totalCount ?? 0);
  }

  protected previousPage(): void {
    this.page.update((page) => page - 1);
  }

  protected nextPage(): void {
    this.page.update((page) => page + 1);
  }

  protected async publish(id: string): Promise<void> {
    await this.run(
      id,
      () => this.socialChallengeAuthoring.publish(id),
      'The challenge is published.',
      'Could not publish the challenge.',
    );
  }

  protected async retire(id: string): Promise<void> {
    await this.run(
      id,
      () => this.socialChallengeAuthoring.retire(id),
      'The challenge is retired.',
      'Could not retire the challenge.',
    );
  }

  protected async reactivate(id: string): Promise<void> {
    await this.run(
      id,
      () => this.socialChallengeAuthoring.reactivate(id),
      'The challenge is published again.',
      'Could not publish the challenge again.',
    );
  }

  protected askForConfirmation(id: string, action: 'archive' | 'delete'): void {
    this.rowError.set(null);
    this.success.set(null);
    this.awaitingConfirmation.set({ id, action });
  }

  protected cancelConfirmation(): void {
    this.awaitingConfirmation.set(null);
  }

  protected async confirm(): Promise<void> {
    const confirmation = this.awaitingConfirmation();
    if (confirmation === null) {
      return;
    }
    this.awaitingConfirmation.set(null);
    if (confirmation.action === 'archive') {
      await this.run(
        confirmation.id,
        () => this.socialChallengeAuthoring.archive(confirmation.id),
        'The challenge is archived.',
        'Could not archive the challenge.',
      );
    } else {
      // Deleting the only challenge on a later page would leave that page empty,
      // so the list then moves back one page instead of reloading the same one.
      const lastOnPage = this.page() > 1 && this.challenges.value()?.items.length === 1;
      await this.run(
        confirmation.id,
        () => this.socialChallengeAuthoring.delete(confirmation.id),
        'The challenge is deleted.',
        'Could not delete the challenge.',
        lastOnPage ? () => this.previousPage() : undefined,
      );
    }
  }

  // Every row action follows the same steps: run the command, show the outcome, reload the list.
  // A changed page number loads its own list, so `afterSuccess` can replace the reload.
  private async run(
    rowId: string,
    command: () => Promise<void>,
    successMessage: string,
    fallbackError: string,
    afterSuccess: () => void = () => this.challenges.reload(),
  ): Promise<void> {
    this.pending.set(true);
    this.rowError.set(null);
    this.success.set(null);
    try {
      await command();
      this.success.set(successMessage);
      afterSuccess();
    } catch (failure) {
      this.rowError.set({ rowId, message: serverMessage(failure, fallbackError) });
    } finally {
      this.pending.set(false);
    }
  }
}
