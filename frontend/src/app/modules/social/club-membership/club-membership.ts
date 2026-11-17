import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';

const BASE_URL = '/api/social/clubs';

@Injectable({ providedIn: 'root' })
export class ClubMembership {
  private readonly http = inject(HttpClient);

  async requestToJoin(clubId: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${clubId}/join-requests`, null));
  }

  async withdrawRequest(clubId: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${BASE_URL}/${clubId}/join-requests`));
  }

  async acceptRequest(clubId: string, requesterId: string): Promise<void> {
    await firstValueFrom(
      this.http.post<void>(`${BASE_URL}/${clubId}/join-requests/${requesterId}/accept`, null),
    );
  }

  async rejectRequest(clubId: string, requesterId: string): Promise<void> {
    await firstValueFrom(
      this.http.post<void>(`${BASE_URL}/${clubId}/join-requests/${requesterId}/reject`, null),
    );
  }

  async invite(clubId: string, inviteeId: string): Promise<void> {
    await firstValueFrom(
      this.http.post<void>(`${BASE_URL}/${clubId}/invitations/${inviteeId}`, null),
    );
  }

  async withdrawInvitation(clubId: string, inviteeId: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${BASE_URL}/${clubId}/invitations/${inviteeId}`));
  }

  async removeMember(clubId: string, memberId: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${BASE_URL}/${clubId}/members/${memberId}`));
  }
}
