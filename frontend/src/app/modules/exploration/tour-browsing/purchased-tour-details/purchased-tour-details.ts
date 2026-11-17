import { HttpErrorResponse, httpResource } from '@angular/common/http';
import { Component, computed, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TourDto } from '../../api/exploration-api-types';
import { TourCard } from '../tour-card/tour-card';

@Component({
  selector: 'app-purchased-tour-details',
  imports: [RouterLink, TourCard],
  templateUrl: './purchased-tour-details.html',
  styleUrl: './purchased-tour-details.scss',
})
export class PurchasedTourDetails {
  readonly id = input.required<string>();

  protected readonly tour = httpResource<TourDto>(
    () => '/api/exploration/tours/purchased/' + encodeURIComponent(this.id()),
  );
  protected readonly errorStatus = computed(() => {
    const error = this.tour.error();
    return error instanceof HttpErrorResponse ? error.status : null;
  });
}
