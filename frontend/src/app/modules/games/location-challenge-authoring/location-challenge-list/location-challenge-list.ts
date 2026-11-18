import { HttpErrorResponse, httpResource } from '@angular/common/http';
import { Component, computed, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PageResult } from '../../../../shared/api/page-result';
import { LocationChallengeSummaryDto } from '../../api/games-api-types';

const PAGE_SIZE = 20;

@Component({
  imports: [RouterLink],
  selector: 'app-location-challenge-list',
  styleUrl: './location-challenge-list.scss',
  templateUrl: './location-challenge-list.html',
})
export class LocationChallengeList {
  protected readonly page = signal(1);

  protected readonly challenges = httpResource<PageResult<LocationChallengeSummaryDto>>(
    () => `/api/games/location-challenges?page=${this.page()}&pageSize=${PAGE_SIZE}`,
  );

  protected readonly challengesErrorStatus = computed(() => {
    const failure = this.challenges.error();
    return failure instanceof HttpErrorResponse ? failure.status : null;
  });

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
