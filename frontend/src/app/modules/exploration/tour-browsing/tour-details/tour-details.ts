import { HttpErrorResponse, httpResource } from '@angular/common/http';
import { Component, computed, inject, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { TourDetailsDto } from '../../api/exploration-api-types';

@Component({
  imports: [RouterLink],
  selector: 'app-tour-details',
  styleUrl: './tour-details.scss',
  templateUrl: './tour-details.html',
})
export class TourDetails {
  private readonly auth = inject(Auth);

  readonly id = input.required<string>();

  protected readonly isExplorer = computed(
    () => this.auth.user()?.roles.includes('explorer') ?? false,
  );

  protected readonly tour = httpResource<TourDetailsDto>(
    () => `/api/exploration/tours/${this.id()}`,
  );

  protected readonly tourErrorStatus = computed(() => {
    const failure = this.tour.error();
    return failure instanceof HttpErrorResponse ? failure.status : null;
  });
}
