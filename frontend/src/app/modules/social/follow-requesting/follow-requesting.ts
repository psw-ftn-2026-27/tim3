import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { SendFollowRequestDto } from '../api/social-api-types';

const BASE_URL = '/api/social/follow-requests';

@Injectable({ providedIn: 'root' })
export class FollowRequesting {
  private readonly http = inject(HttpClient);

  async send(dto: SendFollowRequestDto): Promise<void> {
    await firstValueFrom(this.http.post<void>(BASE_URL, dto));
  }

  async cancel(id: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${BASE_URL}/${id}`));
  }
}
