import { Component, input, output } from '@angular/core';
import { MessageDto } from '../../../shared/api/chats';

@Component({
  selector: 'app-message-list',
  styleUrl: './message-list.scss',
  templateUrl: './message-list.html',
})
export class MessageList {
  // The messages in the order they are shown, oldest first.
  readonly messages = input.required<MessageDto[]>();
  // The user reading the chat; their own messages never get moderation buttons.
  readonly viewerId = input.required<string>();
  // True when the viewer is privileged in this chat and may delete messages and mute senders.
  readonly canModerate = input(false);
  // True while a command runs, so the buttons cannot be clicked twice.
  readonly pending = input(false);

  // The id of the message to delete.
  readonly deleteMessage = output<string>();
  // The id of the user to mute.
  readonly muteSender = output<string>();

  // Moderation is offered only next to messages of others who are not privileged themselves.
  protected canModerateMessage(message: MessageDto): boolean {
    return (
      this.canModerate() && message.senderId !== this.viewerId() && !message.senderIsPrivileged
    );
  }

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
