import { HttpErrorResponse, httpResource } from '@angular/common/http';
import { Component, DestroyRef, computed, inject, input, signal } from '@angular/core';
import { FormField, form, maxLength, required } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { ChatMessagesDto } from '../../shared/api/chats';
import { serverMessage } from '../../shared/util/server-message';
import { Auth } from '../auth/auth';
import { ChatMessaging } from './chat-messaging';
import { MessageList } from './message-list/message-list';

const PAGE_SIZE = 20;
const REFRESH_INTERVAL_MS = 5000;

// One page serves every chat. Exactly one route matches, so exactly one kind of input is set:
// tourId, challengeId, or the two users of a conversation.
@Component({
  imports: [FormField, MessageList, RouterLink],
  selector: 'app-chat',
  styleUrl: './chat.scss',
  templateUrl: './chat.html',
})
export class Chat {
  readonly tourId = input<string>();
  readonly challengeId = input<string>();
  readonly firstUserId = input<string>();
  readonly secondUserId = input<string>();

  protected readonly auth = inject(Auth);
  private readonly chatMessaging = inject(ChatMessaging);

  private readonly baseUrl = computed(() => {
    const tourId = this.tourId();
    if (tourId) {
      return `/api/chat/tours/${tourId}`;
    }
    const challengeId = this.challengeId();
    if (challengeId) {
      return `/api/chat/challenges/${challengeId}`;
    }
    return `/api/chat/users/${this.firstUserId()}/${this.secondUserId()}`;
  });

  // The page the user came from, for the link back.
  protected readonly back = computed(() => {
    if (this.tourId()) {
      return { link: '/exploration', label: 'Back to tours' };
    }
    if (this.challengeId()) {
      return { link: '/games/challenges', label: 'Back to challenges' };
    }
    return { link: '/social/users', label: 'Back to user search' };
  });

  // A logged-out user may not read a chat, so nothing is loaded for them.
  protected readonly messages = httpResource<ChatMessagesDto>(() =>
    this.auth.isLoggedIn() ? `${this.baseUrl()}/messages?page=1&pageSize=${PAGE_SIZE}` : undefined,
  );

  // The server returns the newest messages first; the chat shows them oldest first.
  protected readonly oldestFirst = computed(() =>
    [...(this.messages.value()?.messages.items ?? [])].reverse(),
  );

  // The viewer's standing in this chat, as the server reports it with the messages.
  protected readonly viewerIsPrivileged = computed(
    () => this.messages.value()?.viewerIsPrivileged ?? false,
  );
  protected readonly viewerIsMuted = computed(() => this.messages.value()?.viewerIsMuted ?? false);
  // The server fills this list only for a privileged viewer.
  protected readonly mutedUsers = computed(() => this.messages.value()?.mutedUsers ?? []);
  protected readonly viewerId = computed(() => this.auth.user()?.id ?? '');

  // The server answers 400 when the reader is not one of the two users of a conversation.
  protected readonly accessDenied = computed(() => {
    const failure = this.messages.error();
    return failure instanceof HttpErrorResponse && failure.status === 400;
  });

  // The chat reloads its messages on its own every few seconds, so new messages from others
  // appear without clicking Refresh. The timer stops when the user leaves the page.
  constructor() {
    const timer = setInterval(() => {
      if (!this.accessDenied()) {
        this.messages.reload();
      }
    }, REFRESH_INTERVAL_MS);
    inject(DestroyRef).onDestroy(() => clearInterval(timer));
  }

  protected readonly form = form(signal({ text: '' }), (path) => {
    required(path.text, { message: 'A message cannot be empty.' });
    maxLength(path.text, 1000, { message: 'A message cannot be longer than 1000 characters.' });
  });

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);
  // The reason a delete, mute or unmute failed, shown above the message list.
  protected readonly moderationError = signal<string | null>(null);
  // The reason for an empty or too long message shows only after the user tries to send it.
  protected readonly triedToSend = signal(false);

  protected async send(event: Event): Promise<void> {
    event.preventDefault();
    this.triedToSend.set(true);
    if (!this.form().valid()) {
      return;
    }
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.chatMessaging.send(this.baseUrl(), { text: this.form.text().value() });
      this.form().reset({ text: '' });
      this.triedToSend.set(false);
      this.messages.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not send the message.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected deleteMessage(messageId: string): Promise<void> {
    return this.moderate(
      () => this.chatMessaging.deleteMessage(this.baseUrl(), messageId),
      'Could not delete the message.',
    );
  }

  protected muteUser(userId: string): Promise<void> {
    return this.moderate(
      () => this.chatMessaging.muteUser(this.baseUrl(), userId),
      'Could not mute the user.',
    );
  }

  protected unmuteUser(userId: string): Promise<void> {
    return this.moderate(
      () => this.chatMessaging.unmuteUser(this.baseUrl(), userId),
      'Could not unmute the user.',
    );
  }

  // Runs one moderation command, then reloads the messages and the muted users.
  // On failure the list stays as it was and the server's reason shows above it.
  private async moderate(command: () => Promise<void>, fallback: string): Promise<void> {
    this.pending.set(true);
    this.moderationError.set(null);
    try {
      await command();
      this.messages.reload();
    } catch (failure) {
      this.moderationError.set(serverMessage(failure, fallback));
    } finally {
      this.pending.set(false);
    }
  }
}
