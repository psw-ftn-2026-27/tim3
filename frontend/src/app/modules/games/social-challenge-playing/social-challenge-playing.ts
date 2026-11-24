import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { SetUserPositionDto } from '../../../shared/api/maps';

const BASE_URL = '/api/games/social-challenges';

@Injectable({ providedIn: 'root' })
export class SocialChallengePlaying {
  private readonly http = inject(HttpClient);

  // The explorer moves their position on the play page map; the platform Maps module stores it.
  async setPosition(position: SetUserPositionDto): Promise<void> {
    await firstValueFrom(this.http.put<void>('/api/maps/user-positions/me', position));
  }

  // Starts the challenge's execution, or joins the one already running.
  async activate(id: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/activate`, {}));
  }

  // The server reads the explorer's saved position itself, so the report carries no body.
  async reportPosition(id: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/position-reports`, {}));
  }
}
