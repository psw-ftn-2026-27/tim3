import { HttpErrorResponse, httpResource } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { FormField, form, required } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { PageResult } from '../../../../shared/api/page-result';
import { serverMessage } from '../../../../shared/util/server-message';
import { ClubDetailsDto, ClubUserDto } from '../../api/social-api-types';
import { ClubMembership } from '../club-membership';

const PAGE_SIZE = 20;

@Component({
  imports: [FormField, RouterLink],
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
  // The owner and the moderators resolve join requests and remove regular members.
  protected readonly canModerate = computed(
    () => this.isOwner() || this.club.value()?.role === 'Moderator',
  );

  // Only the owner sees the pending invitations, so nobody else sends this request.
  protected readonly invitations = httpResource<ClubUserDto[]>(() =>
    this.isOwner() ? `/api/social/clubs/${this.id()}/invitations` : undefined,
  );

  protected readonly form = form(signal({ text: '' }), (path) => {
    required(path.text, { message: 'Enter a username.' });
  });

  protected readonly searchedText = signal<string | null>(null);
  protected readonly page = signal(1);

  protected readonly invitees = httpResource<PageResult<ClubUserDto>>(() => {
    const text = this.searchedText();
    if (text === null) {
      return undefined;
    }
    return `/api/social/clubs/invitees?text=${encodeURIComponent(text)}&page=${this.page()}&pageSize=${PAGE_SIZE}`;
  });

  protected readonly pending = signal(false);
  // Each message is shown next to the part of the page whose command failed.
  protected readonly requestError = signal<string | null>(null);
  protected readonly withdrawError = signal<string | null>(null);
  protected readonly resolveError = signal<string | null>(null);
  protected readonly memberError = signal<string | null>(null);
  protected readonly invitationError = signal<string | null>(null);
  protected readonly inviteError = signal<{ userId: string; message: string } | null>(null);
  // The member whose row asks the owner to confirm the transfer of ownership.
  protected readonly confirmingOwnerId = signal<string | null>(null);

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

  protected async removeMember(memberId: string): Promise<void> {
    this.clearErrors();
    this.pending.set(true);
    try {
      await this.clubMembership.removeMember(this.id(), memberId);
      this.reload();
    } catch (failure) {
      this.memberError.set(serverMessage(failure, 'Could not remove the member.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async promoteToModerator(memberId: string): Promise<void> {
    this.clearErrors();
    this.pending.set(true);
    try {
      await this.clubMembership.promoteToModerator(this.id(), memberId);
      this.club.reload();
    } catch (failure) {
      this.memberError.set(serverMessage(failure, 'Could not make the member a moderator.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async transferOwnership(newOwnerId: string): Promise<void> {
    this.clearErrors();
    this.confirmingOwnerId.set(null);
    this.pending.set(true);
    try {
      await this.clubMembership.transferOwnership(this.id(), newOwnerId);
      this.club.reload();
    } catch (failure) {
      this.memberError.set(serverMessage(failure, 'Could not transfer the ownership.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected search(event: Event): void {
    event.preventDefault();
    this.clearErrors();
    this.page.set(1);
    this.searchedText.set(this.form.text().value());
  }

  protected hasNextPage(): boolean {
    return this.page() * PAGE_SIZE < (this.invitees.value()?.totalCount ?? 0);
  }

  protected previousPage(): void {
    this.clearErrors();
    this.page.update((page) => page - 1);
  }

  protected nextPage(): void {
    this.clearErrors();
    this.page.update((page) => page + 1);
  }

  protected async invite(user: ClubUserDto): Promise<void> {
    this.clearErrors();
    this.pending.set(true);
    try {
      await this.clubMembership.invite(this.id(), user.userId);
      this.reload();
    } catch (failure) {
      this.inviteError.set({
        userId: user.userId,
        message: `${user.displayName}: ${serverMessage(failure, 'Could not send the invitation.')}`,
      });
    } finally {
      this.pending.set(false);
    }
  }

  protected async withdrawInvitation(inviteeId: string): Promise<void> {
    this.clearErrors();
    this.pending.set(true);
    try {
      await this.clubMembership.withdrawInvitation(this.id(), inviteeId);
      this.reload();
    } catch (failure) {
      this.invitationError.set(serverMessage(failure, 'Could not withdraw the invitation.'));
    } finally {
      this.pending.set(false);
    }
  }

  private reload(): void {
    this.club.reload();
    this.invitations.reload();
  }

  private clearErrors(): void {
    this.requestError.set(null);
    this.withdrawError.set(null);
    this.resolveError.set(null);
    this.memberError.set(null);
    this.invitationError.set(null);
    this.inviteError.set(null);
  }
}
