import { HttpErrorResponse, httpResource } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { serverMessage } from '../../../../shared/util/server-message';
import { ClubDetailsDto } from '../../api/social-api-types';
import { ClubMembership } from '../club-membership';

@Component({
  imports: [RouterLink],
  selector: 'app-club-detail',
  styleUrl: './club-detail.scss',
  templateUrl: './club-detail.html',
})
export class ClubDetail {
  private readonly clubMembership = inject(ClubMembership);
  private readonly auth = inject(Auth);

  readonly id = input.required<string>();

  protected readonly club = httpResource<ClubDetailsDto>(() => `/api/social/clubs/${this.id()}`);

  protected readonly clubErrorStatus = computed(() => {
    const failure = this.club.error();
    return failure instanceof HttpErrorResponse ? failure.status : null;
  });

  // Hiding the buttons is only display; the server decides who may send or resolve a request.
  protected readonly isExplorer = computed(
    () => this.auth.user()?.roles.includes('explorer') ?? false,
  );
  protected readonly isOwner = computed(
    () => this.club.value()?.ownerId === this.auth.user()?.id,
  );

  protected readonly pending = signal(false);
  // Each message is shown next to the part of the page whose command failed.
  protected readonly requestError = signal<string | null>(null);
  protected readonly withdrawError = signal<string | null>(null);
  protected readonly resolveError = signal<string | null>(null);

  protected async requestToJoin(): Promise<void> {
    this.clearErrors();
    this.pending.set(true);
    try {
      await this.clubMembership.requestToJoin(this.id());
      this.club.reload();
    } catch (failure) {
      this.requestError.set(serverMessage(failure, 'Could not send the join request.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async withdrawRequest(): Promise<void> {
    this.clearErrors();
    this.pending.set(true);
    try {
      await this.clubMembership.withdrawRequest(this.id());
      this.club.reload();
    } catch (failure) {
      this.withdrawError.set(serverMessage(failure, 'Could not withdraw the join request.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async accept(requesterId: string): Promise<void> {
    this.clearErrors();
    this.pending.set(true);
    try {
      await this.clubMembership.acceptRequest(this.id(), requesterId);
      this.club.reload();
    } catch (failure) {
      this.resolveError.set(serverMessage(failure, 'Could not accept the join request.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async reject(requesterId: string): Promise<void> {
    this.clearErrors();
    this.pending.set(true);
    try {
      await this.clubMembership.rejectRequest(this.id(), requesterId);
      this.club.reload();
    } catch (failure) {
      this.resolveError.set(serverMessage(failure, 'Could not reject the join request.'));
    } finally {
      this.pending.set(false);
    }
  }

  private clearErrors(): void {
    this.requestError.set(null);
    this.withdrawError.set(null);
    this.resolveError.set(null);
  }
}
