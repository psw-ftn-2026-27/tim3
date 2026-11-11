import { httpResource } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PageResult } from '../../../../shared/api/page-result';
import { serverMessage } from '../../../../shared/util/server-message';
import {
  ConnectedUserDto,
  IncomingFollowRequestDto,
  ProfileSummaryDto,
  SentFollowRequestDto,
} from '../../api/social-api-types';
import { FollowManagement } from '../follow-management';

const PAGE_SIZE = 20;

@Component({
  imports: [RouterLink],
  selector: 'app-profile',
  styleUrl: './profile.scss',
  templateUrl: './profile.html',
})
export class Profile {
  private readonly followManagement = inject(FollowManagement);

  protected readonly section = signal<'followers' | 'following' | 'sent' | 'incoming' | null>(
    null,
  );
  protected readonly page = signal(1);

  protected readonly summary = httpResource<ProfileSummaryDto>(() => '/api/social/profile/summary');

  // Each list loads only while its section is open.
  protected readonly followers = httpResource<PageResult<ConnectedUserDto>>(() =>
    this.section() === 'followers'
      ? `/api/social/profile/followers?page=${this.page()}&pageSize=${PAGE_SIZE}`
      : undefined,
  );

  protected readonly following = httpResource<PageResult<ConnectedUserDto>>(() =>
    this.section() === 'following'
      ? `/api/social/profile/following?page=${this.page()}&pageSize=${PAGE_SIZE}`
      : undefined,
  );

  protected readonly sent = httpResource<PageResult<SentFollowRequestDto>>(() =>
    this.section() === 'sent'
      ? `/api/social/follow-requests/sent?page=${this.page()}&pageSize=${PAGE_SIZE}`
      : undefined,
  );

  protected readonly incoming = httpResource<PageResult<IncomingFollowRequestDto>>(() =>
    this.section() === 'incoming'
      ? `/api/social/follow-requests/incoming?page=${this.page()}&pageSize=${PAGE_SIZE}`
      : undefined,
  );

  protected readonly pending = signal(false);
  protected readonly rowError = signal<{ rowId: string; message: string } | null>(null);

  protected toggle(section: 'followers' | 'following' | 'sent' | 'incoming'): void {
    this.rowError.set(null);
    this.page.set(1);
    this.section.update((open) => (open === section ? null : section));
  }

  protected hasNextPage(totalCount: number | undefined): boolean {
    return this.page() * PAGE_SIZE < (totalCount ?? 0);
  }

  protected previousPage(): void {
    this.rowError.set(null);
    this.page.update((page) => page - 1);
  }

  protected nextPage(): void {
    this.rowError.set(null);
    this.page.update((page) => page + 1);
  }

  protected async remove(user: ConnectedUserDto): Promise<void> {
    this.pending.set(true);
    this.rowError.set(null);
    try {
      await this.followManagement.removeFollower(user.userId);
    } catch (failure) {
      this.rowError.set({
        rowId: user.userId,
        message: `${user.displayName}: ${serverMessage(failure, 'Could not remove the follower.')}`,
      });
    } finally {
      this.pending.set(false);
      this.reloadAll();
    }
  }

  protected async unfollow(user: ConnectedUserDto): Promise<void> {
    this.pending.set(true);
    this.rowError.set(null);
    try {
      await this.followManagement.unfollow(user.userId);
    } catch (failure) {
      this.rowError.set({
        rowId: user.userId,
        message: `${user.displayName}: ${serverMessage(failure, 'Could not unfollow the user.')}`,
      });
    } finally {
      this.pending.set(false);
      this.reloadAll();
    }
  }

  protected async cancel(request: SentFollowRequestDto): Promise<void> {
    this.pending.set(true);
    this.rowError.set(null);
    try {
      await this.followManagement.cancel(request.id);
    } catch (failure) {
      this.rowError.set({
        rowId: request.id,
        message: `${request.recipientDisplayName}: ${serverMessage(failure, 'Could not cancel the follow request.')}`,
      });
    } finally {
      this.pending.set(false);
      this.reloadAll();
    }
  }

  protected async accept(request: IncomingFollowRequestDto): Promise<void> {
    this.pending.set(true);
    this.rowError.set(null);
    try {
      await this.followManagement.accept(request.id);
    } catch (failure) {
      this.rowError.set({
        rowId: request.id,
        message: `${request.senderDisplayName}: ${serverMessage(failure, 'Could not accept the follow request.')}`,
      });
    } finally {
      this.pending.set(false);
      this.reloadAll();
    }
  }

  protected async reject(request: IncomingFollowRequestDto): Promise<void> {
    this.pending.set(true);
    this.rowError.set(null);
    try {
      await this.followManagement.reject(request.id);
    } catch (failure) {
      this.rowError.set({
        rowId: request.id,
        message: `${request.senderDisplayName}: ${serverMessage(failure, 'Could not reject the follow request.')}`,
      });
    } finally {
      this.pending.set(false);
      this.reloadAll();
    }
  }

  protected date(iso: string): string {
    return new Date(iso).toLocaleDateString('sr-RS');
  }

  // Accepting adds a follower, so every command reloads both counts and the open list.
  private reloadAll(): void {
    this.summary.reload();
    this.followers.reload();
    this.following.reload();
    this.sent.reload();
    this.incoming.reload();
  }
}
