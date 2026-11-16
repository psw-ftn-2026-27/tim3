import { Component, input } from '@angular/core';
import { MessageDto } from '../../../shared/api/chats';

@Component({
  selector: 'app-message-list',
  styleUrl: './message-list.scss',
  templateUrl: './message-list.html',
})
export class MessageList {
  // The messages in the order they are shown, oldest first.
  readonly messages = input.required<MessageDto[]>();

  // "Nov 15, 2026, 21:42"
  protected when(sentAt: string): string {
    return new Date(sentAt).toLocaleString('en-US', {
      day: 'numeric',
      hour: '2-digit',
      hour12: false,
      minute: '2-digit',
      month: 'short',
      year: 'numeric',
    });
  }
}
