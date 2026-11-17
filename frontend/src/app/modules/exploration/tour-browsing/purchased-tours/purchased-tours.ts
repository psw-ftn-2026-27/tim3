import { HttpErrorResponse, httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { PageResult } from '../../../../shared/api/page-result';
import { TourDto } from '../../api/exploration-api-types';
import { TourCard } from '../tour-card/tour-card';

@Component({
  imports: [RouterLink, TourCard],
  selector: 'app-purchased-tours',
  styleUrl: './purchased-tours.scss',
  templateUrl: './purchased-tours.html',
})
export class PurchasedTours {
  private readonly auth = inject(Auth);
  protected readonly isExplorer = computed(
    () => this.auth.user()?.roles.includes('explorer') ?? false,
  );
  private readonly pageSize = 20;
  protected readonly page = signal(1);
  protected readonly tours = httpResource<PageResult<TourDto>>(
    () => `/api/exploration/tours/purchased?page=${this.page()}&pageSize=${this.pageSize}`,
  );
  protected readonly lastPage = computed(() =>
    Math.ceil((this.tours.value()?.totalCount ?? 0) / this.pageSize),
  );
  protected readonly toursErrorStatus = computed(() => {
    const failure = this.tours.error();
    return failure instanceof HttpErrorResponse ? failure.status : null;
  });
}
