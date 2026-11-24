import { HttpErrorResponse, httpResource } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { UserPositionDto } from '../../../../shared/api/maps';
import {
  LocationMap,
  MapCircle,
  MapLocation,
} from '../../../../shared/map/location-map/location-map';
import { serverMessage } from '../../../../shared/util/server-message';
import { LocationChallengeHuntDto } from '../../api/games-api-types';
import { LocationChallengeSolving } from '../location-challenge-solving';

@Component({
  imports: [RouterLink, LocationMap],
  selector: 'app-location-challenge-hunt',
  styleUrl: './location-challenge-hunt.scss',
  templateUrl: './location-challenge-hunt.html',
})
export class LocationChallengeHunt {
  private readonly locationChallengeSolving = inject(LocationChallengeSolving);

  readonly id = input.required<string>();

  protected readonly hunt = httpResource<LocationChallengeHuntDto>(
    () => `/api/games/location-challenges/solving/${this.id()}`,
  );

  protected readonly huntErrorStatus = computed(() => {
    const failure = this.hunt.error();
    return failure instanceof HttpErrorResponse ? failure.status : null;
  });

  // Only discovered points come with a location, so only they are pins on the map.
  protected readonly discoveredLocations = computed<MapLocation[]>(() => {
    const locations: MapLocation[] = [];
    for (const point of this.hunt.value()?.hiddenPoints ?? []) {
      if (point.latitude !== null && point.longitude !== null) {
        locations.push({ latitude: point.latitude, longitude: point.longitude });
      }
    }
    return locations;
  });

  protected readonly circle = computed<MapCircle | null>(() => {
    const searchCircle = this.hunt.value()?.searchCircle ?? null;
    if (searchCircle === null) {
      return null;
    }
    return {
      center: { latitude: searchCircle.centerLatitude, longitude: searchCircle.centerLongitude },
      radiusInMetres: searchCircle.radius,
    };
  });

  // The explorer's saved position. While it loads or when reading fails, the map draws no pin.
  protected readonly userPosition = httpResource<UserPositionDto>(
    () => '/api/maps/user-positions/me',
  );
  protected readonly position = computed<MapLocation | null>(() => {
    if (!this.userPosition.hasValue()) {
      return null;
    }
    const position = this.userPosition.value();
    return { latitude: position.latitude, longitude: position.longitude };
  });
  // Only 404 means that the explorer has not set a position yet.
  protected readonly positionMissing = computed(() => {
    const failure = this.userPosition.error();
    return failure instanceof HttpErrorResponse && failure.status === 404;
  });
  protected readonly positionError = signal<string | null>(null);

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly success = signal<string | null>(null);

  // The pin moves only after the server saves the position and the page reads it again.
  protected async setPosition(position: MapLocation): Promise<void> {
    this.positionError.set(null);
    try {
      await this.locationChallengeSolving.setPosition(position);
      this.userPosition.reload();
    } catch (failure) {
      this.positionError.set(serverMessage(failure, 'Could not save your position.'));
    }
  }

  // The check only confirms; the page reads the challenge again to show the new point or the solution.
  protected async checkLocation(): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    this.success.set(null);
    try {
      await this.locationChallengeSolving.checkLocation(this.id());
      this.success.set('You discovered a hidden point.');
      this.hunt.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not check your location.'));
    } finally {
      this.pending.set(false);
    }
  }
}
