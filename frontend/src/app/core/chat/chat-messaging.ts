import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { SendMessageDto } from '../../shared/api/chats';

@Injectable({ providedIn: 'root' })
export class ChatMessaging {
  private readonly http = inject(HttpClient);

  // baseUrl names the chat, for example /api/chat/tours/<id>.
  async send(baseUrl: string, message: SendMessageDto): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${baseUrl}/messages`, message));
  }

  async deleteMessage(baseUrl: string, messageId: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${baseUrl}/messages/${messageId}`));
  }

  async muteUser(baseUrl: string, userId: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${baseUrl}/muted-users/${userId}`, null));
  }

  async unmuteUser(baseUrl: string, userId: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${baseUrl}/muted-users/${userId}`));
  }
}
