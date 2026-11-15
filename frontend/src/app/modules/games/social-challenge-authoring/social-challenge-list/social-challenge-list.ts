import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { PageResult } from '../../../../shared/api/page-result';
import { SocialChallengeDto } from '../../api/games-api-types';

const PAGE_SIZE = 20;

@Component({
  imports: [RouterLink],
  selector: 'app-social-challenge-list',
  styleUrl: './social-challenge-list.scss',
  templateUrl: './social-challenge-list.html',
})
export class SocialChallengeList {
  private readonly auth = inject(Auth);

  protected readonly isAdministrator = computed(
    () => this.auth.user()?.roles.includes('administrator') ?? false,
  );

  protected readonly page = signal(1);

  protected readonly challenges = httpResource<PageResult<SocialChallengeDto>>(
    () => `/api/games/social-challenges?page=${this.page()}&pageSize=${PAGE_SIZE}`,
  );

  protected hasNextPage(totalCount: number | undefined): boolean {
    return this.page() * PAGE_SIZE < (totalCount ?? 0);
  }

  protected previousPage(): void {
    this.page.update((page) => page - 1);
  }

  protected nextPage(): void {
    this.page.update((page) => page + 1);
  }
}
