import { httpResource } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormField, form, required } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { PageResult } from '../../../../shared/api/page-result';
import { serverMessage } from '../../../../shared/util/server-message';
import { FoundUserDto } from '../../api/social-api-types';
import { FollowRequesting } from '../follow-requesting';

const PAGE_SIZE = 20;

@Component({
  imports: [FormField, RouterLink],
  selector: 'app-user-search',
  styleUrl: './user-search.scss',
  templateUrl: './user-search.html',
})
export class UserSearch {
  private readonly followRequesting = inject(FollowRequesting);

  protected readonly form = form(signal({ text: '' }), (path) => {
    required(path.text, { message: 'Enter a username.' });
  });

  protected readonly searchedText = signal<string | null>(null);
  protected readonly page = signal(1);

  protected readonly users = httpResource<PageResult<FoundUserDto>>(() => {
    const text = this.searchedText();
    if (text === null) {
      return undefined;
    }
    return `/api/social/users?text=${encodeURIComponent(text)}&page=${this.page()}&pageSize=${PAGE_SIZE}`;
  });

  protected readonly pending = signal(false);
  protected readonly rowError = signal<{ userId: string; message: string } | null>(null);

  protected search(event: Event): void {
    event.preventDefault();
    this.rowError.set(null);
    this.page.set(1);
    this.searchedText.set(this.form.text().value());
  }

  protected hasNextPage(): boolean {
    return this.page() * PAGE_SIZE < (this.users.value()?.totalCount ?? 0);
  }

  protected previousPage(): void {
    this.rowError.set(null);
    this.page.update((page) => page - 1);
  }

  protected nextPage(): void {
    this.rowError.set(null);
    this.page.update((page) => page + 1);
  }

  protected async follow(user: FoundUserDto): Promise<void> {
    this.pending.set(true);
    this.rowError.set(null);
    try {
      await this.followRequesting.send({ recipientId: user.userId });
    } catch (failure) {
      this.rowError.set({
        userId: user.userId,
        message: `${user.displayName}: ${serverMessage(failure, 'Could not send the follow request.')}`,
      });
    } finally {
      this.pending.set(false);
      this.users.reload();
    }
  }

  protected async cancel(user: FoundUserDto, requestId: string): Promise<void> {
    this.pending.set(true);
    this.rowError.set(null);
    try {
      await this.followRequesting.cancel(requestId);
    } catch (failure) {
      this.rowError.set({
        userId: user.userId,
        message: `${user.displayName}: ${serverMessage(failure, 'Could not cancel the follow request.')}`,
      });
    } finally {
      this.pending.set(false);
      this.users.reload();
    }
  }

  protected date(iso: string): string {
    return new Date(iso).toLocaleDateString('sr-RS');
  }
}
