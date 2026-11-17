import { Component, computed, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TourDto } from '../../api/exploration-api-types';

@Component({
  imports: [RouterLink],
  selector: 'app-tour-card',
  styleUrl: './tour-card.scss',
  templateUrl: './tour-card.html',
})
export class TourCard {
  readonly tour = input.required<TourDto>();
  readonly priceInEc = input<number | null>();
  readonly showBrowsingActions = input(true);
  readonly showPurchasedDetails = input(false);
  readonly canAddToCart = input(false);

  // How many of the three difficulty bars are filled.
  protected readonly level = computed(() => {
    const levels = { Easy: 1, Moderate: 2, Hard: 3 };
    return levels[this.tour().difficulty];
  });
}
