import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';

const PROFILE_URL = '/api/social/profile';
const REQUESTS_URL = '/api/social/follow-requests';

@Injectable({ providedIn: 'root' })
export class FollowManagement {
  private readonly http = inject(HttpClient);

  async removeFollower(followerId: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${PROFILE_URL}/followers/${followerId}`));
  }

  async unfollow(followedId: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${PROFILE_URL}/following/${followedId}`));
  }

  async cancel(requestId: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${REQUESTS_URL}/${requestId}`));
  }

  async accept(requestId: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${REQUESTS_URL}/${requestId}/accept`, {}));
  }

  async reject(requestId: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${REQUESTS_URL}/${requestId}/reject`, {}));
  }
}
