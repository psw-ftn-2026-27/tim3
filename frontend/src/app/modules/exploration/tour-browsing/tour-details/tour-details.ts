import { HttpErrorResponse, httpResource } from '@angular/common/http';
import { Component, computed, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TourDetailsDto } from '../../api/exploration-api-types';

@Component({
  imports: [RouterLink],
  selector: 'app-tour-details',
  styleUrl: './tour-details.scss',
  templateUrl: './tour-details.html',
})
export class TourDetails {
  readonly id = input.required<string>();

  protected readonly tour = httpResource<TourDetailsDto>(
    () => `/api/exploration/tours/${this.id()}`,
  );

  protected readonly tourErrorStatus = computed(() => {
    const failure = this.tour.error();
    return failure instanceof HttpErrorResponse ? failure.status : null;
  });
}
