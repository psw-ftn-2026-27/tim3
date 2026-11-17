import { httpResource } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { ReceivedInvitationDto } from '../../api/social-api-types';
import { ClubRoster } from '../club-roster';

@Component({
  imports: [RouterLink],
  selector: 'app-my-invitations',
  styleUrl: './my-invitations.scss',
  templateUrl: './my-invitations.html',
})
export class MyInvitations {
  private readonly clubRoster = inject(ClubRoster);

  protected readonly invitations = httpResource<ReceivedInvitationDto[]>(
    () => '/api/social/clubs/invitations',
  );

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async accept(clubId: string): Promise<void> {
    this.error.set(null);
    this.pending.set(true);
    try {
      await this.clubRoster.acceptInvitation(clubId);
      this.invitations.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not accept the invitation.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async decline(clubId: string): Promise<void> {
    this.error.set(null);
    this.pending.set(true);
    try {
      await this.clubRoster.declineInvitation(clubId);
      this.invitations.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not decline the invitation.'));
    } finally {
      this.pending.set(false);
    }
  }
}
