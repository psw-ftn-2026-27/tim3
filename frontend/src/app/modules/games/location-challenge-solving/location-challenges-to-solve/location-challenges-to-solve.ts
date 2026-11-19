import { HttpErrorResponse, httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { UserPositionDto } from '../../../../shared/api/maps';
import { PageResult } from '../../../../shared/api/page-result';
import { LocationMap, MapLocation } from '../../../../shared/map/location-map/location-map';
import { serverMessage } from '../../../../shared/util/server-message';
import { LocationChallengeToSolveDto } from '../../api/games-api-types';
import { LocationChallengeSolving } from '../location-challenge-solving';

const PAGE_SIZE = 20;

@Component({
  imports: [RouterLink, LocationMap],
  selector: 'app-location-challenges-to-solve',
  styleUrl: './location-challenges-to-solve.scss',
  templateUrl: './location-challenges-to-solve.html',
})
export class LocationChallengesToSolve {
  private readonly locationChallengeSolving = inject(LocationChallengeSolving);
  private readonly router = inject(Router);

  protected readonly page = signal(1);

  protected readonly challenges = httpResource<PageResult<LocationChallengeToSolveDto>>(
    () => `/api/games/location-challenges/solving?page=${this.page()}&pageSize=${PAGE_SIZE}`,
  );

  protected readonly challengesErrorStatus = computed(() => {
    const failure = this.challenges.error();
    return failure instanceof HttpErrorResponse ? failure.status : null;
  });

  // The map shows the start location of every challenge on the page.
  protected readonly startLocations = computed<MapLocation[]>(() =>
    (this.challenges.value()?.items ?? []).map((challenge) => ({
      latitude: challenge.startLatitude,
      longitude: challenge.startLongitude,
    })),
  );

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

  protected hasNextPage(totalCount: number | undefined): boolean {
    return this.page() * PAGE_SIZE < (totalCount ?? 0);
  }

  protected previousPage(): void {
    this.page.update((page) => page - 1);
  }

  protected nextPage(): void {
    this.page.update((page) => page + 1);
  }

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

  protected async activate(id: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.locationChallengeSolving.activate(id);
      await this.router.navigate(['/games/location-challenges', id, 'play']);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not activate the challenge.'));
    } finally {
      this.pending.set(false);
    }
  }
}
