import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';

const BASE_URL = '/api/social/clubs';

@Injectable({ providedIn: 'root' })
export class ClubRoster {
  private readonly http = inject(HttpClient);

  async acceptInvitation(clubId: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${clubId}/invitations/accept`, null));
  }

  async declineInvitation(clubId: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${clubId}/invitations/decline`, null));
  }
}
