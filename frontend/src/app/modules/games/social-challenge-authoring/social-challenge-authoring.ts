import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import {
  CreateSocialChallengeDto,
  SocialChallengeDto,
  UpdateSocialChallengeDto,
} from '../api/games-api-types';

const BASE_URL = '/api/games/social-challenges';

@Injectable({ providedIn: 'root' })
export class SocialChallengeAuthoring {
  private readonly http = inject(HttpClient);

  async create(dto: CreateSocialChallengeDto): Promise<SocialChallengeDto> {
    return firstValueFrom(this.http.post<SocialChallengeDto>(BASE_URL, dto));
  }

  async update(id: string, dto: UpdateSocialChallengeDto): Promise<void> {
    await firstValueFrom(this.http.put<void>(`${BASE_URL}/${id}`, dto));
  }

  async publish(id: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/publish`, {}));
  }

  async retire(id: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/retire`, {}));
  }

  async reactivate(id: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/reactivate`, {}));
  }

  async archive(id: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/archive`, {}));
  }

  async delete(id: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${BASE_URL}/${id}`));
  }
}
