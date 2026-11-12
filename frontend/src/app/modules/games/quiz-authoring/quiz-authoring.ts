import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import {
  CreateQuestionDto,
  CreateQuizDto,
  UpdateQuestionDto,
  UpdateQuizDetailsDto,
} from '../api/games-api-types';

const BASE_URL = '/api/games/quizzes';

@Injectable({ providedIn: 'root' })
export class QuizAuthoring {
  private readonly http = inject(HttpClient);

  async create(dto: CreateQuizDto): Promise<string> {
    return firstValueFrom(this.http.post<string>(BASE_URL, dto));
  }

  async updateDetails(id: string, dto: UpdateQuizDetailsDto): Promise<void> {
    await firstValueFrom(this.http.put<void>(`${BASE_URL}/${id}`, dto));
  }

  async addQuestion(id: string, dto: CreateQuestionDto): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/questions`, dto));
  }

  async updateQuestion(id: string, questionId: string, dto: UpdateQuestionDto): Promise<void> {
    await firstValueFrom(this.http.put<void>(`${BASE_URL}/${id}/questions/${questionId}`, dto));
  }

  async removeQuestion(id: string, questionId: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${BASE_URL}/${id}/questions/${questionId}`));
  }

  async publish(id: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/publish`, {}));
  }

  async retire(id: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/retire`, {}));
  }

  async archive(id: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/archive`, {}));
  }
}
