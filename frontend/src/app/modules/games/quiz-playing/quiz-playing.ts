import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { SetUserPositionDto } from '../../../shared/api/maps';
import { serverMessage } from '../../../shared/util/server-message';
import { SubmitAnswerDto } from '../api/games-api-types';

const BASE_URL = '/api/games/quizzes';

const SOMETHING_WENT_WRONG = 'Something went wrong. Please try again.';

// The message a failed quiz command shows. A rule the server refused (400) or a quiz it does not
// offer (404) carries the server's own message; anything else (no session, 500, no network) gets
// one general message, because its server text is not meant for the explorer.
export function commandFailure(error: unknown): string {
  const status = error instanceof HttpErrorResponse ? error.status : null;
  return status === 400 || status === 404 ? serverMessage(error, SOMETHING_WENT_WRONG) : SOMETHING_WENT_WRONG;
}

@Injectable({ providedIn: 'root' })
export class QuizPlaying {
  private readonly http = inject(HttpClient);

  // The explorer sets their position on the Quizzes tab map; the platform Maps module stores it.
  async setPosition(position: SetUserPositionDto): Promise<void> {
    await firstValueFrom(this.http.put<void>('/api/maps/user-positions/me', position));
  }

  async activate(id: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/activate`, {}));
  }

  async submitAnswer(id: string, dto: SubmitAnswerDto): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/answers`, dto));
  }
}
