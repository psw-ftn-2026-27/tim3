import { httpResource } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PageResult } from '../../../../shared/api/page-result';
import { serverMessage } from '../../../../shared/util/server-message';
import { IncomingFollowRequestDto, SentFollowRequestDto } from '../../api/social-api-types';
import { FollowRequesting } from '../follow-requesting';

const PAGE_SIZE = 20;

@Component({
  imports: [RouterLink],
  selector: 'app-follow-requests',
  styleUrl: './follow-requests.scss',
  templateUrl: './follow-requests.html',
})
export class FollowRequests {
  private readonly followRequesting = inject(FollowRequesting);

  protected readonly sentPage = signal(1);
  protected readonly incomingPage = signal(1);

  protected readonly sent = httpResource<PageResult<SentFollowRequestDto>>(
    () => `/api/social/follow-requests/sent?page=${this.sentPage()}&pageSize=${PAGE_SIZE}`,
  );

  protected readonly incoming = httpResource<PageResult<IncomingFollowRequestDto>>(
    () => `/api/social/follow-requests/incoming?page=${this.incomingPage()}&pageSize=${PAGE_SIZE}`,
  );

  protected readonly pending = signal(false);
  protected readonly rowError = signal<{ requestId: string; message: string } | null>(null);

  protected hasNextSentPage(): boolean {
    return this.sentPage() * PAGE_SIZE < (this.sent.value()?.totalCount ?? 0);
  }

  protected hasNextIncomingPage(): boolean {
    return this.incomingPage() * PAGE_SIZE < (this.incoming.value()?.totalCount ?? 0);
  }

  protected changeSentPage(step: number): void {
    this.rowError.set(null);
    this.sentPage.update((page) => page + step);
  }

  protected changeIncomingPage(step: number): void {
    this.rowError.set(null);
    this.incomingPage.update((page) => page + step);
  }

  protected async cancel(request: SentFollowRequestDto): Promise<void> {
    this.pending.set(true);
    this.rowError.set(null);
    try {
      await this.followRequesting.cancel(request.id);
    } catch (failure) {
      this.rowError.set({
        requestId: request.id,
        message: `${request.recipientDisplayName}: ${serverMessage(failure, 'Could not cancel the follow request.')}`,
      });
    } finally {
      this.pending.set(false);
      this.reloadLists();
    }
  }

  protected async accept(request: IncomingFollowRequestDto): Promise<void> {
    this.pending.set(true);
    this.rowError.set(null);
    try {
      await this.followRequesting.accept(request.id);
    } catch (failure) {
      this.rowError.set({
        requestId: request.id,
        message: `${request.senderDisplayName}: ${serverMessage(failure, 'Could not accept the follow request.')}`,
      });
    } finally {
      this.pending.set(false);
      this.reloadLists();
    }
  }

  protected async reject(request: IncomingFollowRequestDto): Promise<void> {
    this.pending.set(true);
    this.rowError.set(null);
    try {
      await this.followRequesting.reject(request.id);
    } catch (failure) {
      this.rowError.set({
        requestId: request.id,
        message: `${request.senderDisplayName}: ${serverMessage(failure, 'Could not reject the follow request.')}`,
      });
    } finally {
      this.pending.set(false);
      this.reloadLists();
    }
  }

  protected date(iso: string): string {
    return new Date(iso).toLocaleDateString('sr-RS');
  }

  private reloadLists(): void {
    this.sent.reload();
    this.incoming.reload();
  }
}
