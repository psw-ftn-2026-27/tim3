import { HttpErrorResponse, httpResource } from '@angular/common/http';
import { Component, computed, inject, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { LocationMap, MapLocation } from '../../../../shared/map/location-map/location-map';
import { TourPreviewDto } from '../../api/exploration-api-types';

@Component({
  imports: [LocationMap, RouterLink],
  selector: 'app-tour-preview',
  styleUrl: './tour-preview.scss',
  templateUrl: './tour-preview.html',
})
export class TourPreview {
  private readonly auth = inject(Auth);
  protected readonly isExplorer = computed(() => this.auth.user()?.roles.includes('explorer') ?? false);

  readonly id = input.required<string>();

  protected readonly tour = httpResource<TourPreviewDto>(
    () => `/api/exploration/tours/${this.id()}`,
  );

  protected readonly tourErrorStatus = computed(() => {
    const failure = this.tour.error();
    return failure instanceof HttpErrorResponse ? failure.status : null;
  });

  // The server sends only the key points visible without purchase, so every one gets a pin.
  protected readonly locations = computed<MapLocation[]>(() =>
    (this.tour.value()?.keyPoints ?? []).map((point) => point.location),
  );

  // The stored length of the whole tour; 0 means it was never measured.
  protected readonly lengthText = computed(() => {
    const length = this.tour.value()?.length;
    if (length === undefined || length.kilometres === 0) {
      return 'Length not measured yet';
    }
    return length.isStraightLine
      ? `about ${length.kilometres.toFixed(2)} km (straight line)`
      : `${length.kilometres.toFixed(2)} km`;
  });
}
