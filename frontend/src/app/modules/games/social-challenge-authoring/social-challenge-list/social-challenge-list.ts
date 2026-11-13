import { httpResource } from '@angular/common/http';
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

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);
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
      () => this.socialChallengeAuthoring.publish(id),
      'Izazov je objavljen.',
      'Izazov ne može da se objavi.',
    );
  }

  protected async retire(id: string): Promise<void> {
    await this.run(
      () => this.socialChallengeAuthoring.retire(id),
      'Izazov je penzionisan.',
      'Izazov ne može da se penzioniše.',
    );
  }

  protected async reactivate(id: string): Promise<void> {
    await this.run(
      () => this.socialChallengeAuthoring.reactivate(id),
      'Izazov je ponovo objavljen.',
      'Izazov ne može ponovo da se objavi.',
    );
  }

  protected askForConfirmation(id: string, action: 'archive' | 'delete'): void {
    this.error.set(null);
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
        () => this.socialChallengeAuthoring.archive(confirmation.id),
        'Izazov je arhiviran.',
        'Izazov ne može da se arhivira.',
      );
    } else {
      // Deleting the only challenge on a later page would leave that page empty,
      // so the list then moves back one page instead of reloading the same one.
      const lastOnPage = this.page() > 1 && this.challenges.value()?.items.length === 1;
      await this.run(
        () => this.socialChallengeAuthoring.delete(confirmation.id),
        'Izazov je obrisan.',
        'Izazov ne može da se obriše.',
        lastOnPage ? () => this.previousPage() : undefined,
      );
    }
  }

  // Every row action follows the same steps: run the command, show the outcome, reload the list.
  // A changed page number loads its own list, so `afterSuccess` can replace the reload.
  private async run(
    command: () => Promise<void>,
    successMessage: string,
    fallbackError: string,
    afterSuccess: () => void = () => this.challenges.reload(),
  ): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    this.success.set(null);
    try {
      await command();
      this.success.set(successMessage);
      afterSuccess();
    } catch (failure) {
      this.error.set(serverMessage(failure, fallbackError));
    } finally {
      this.pending.set(false);
    }
  }
}
