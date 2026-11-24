import { HttpErrorResponse, httpResource } from '@angular/common/http';
import { Component, DestroyRef, computed, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { UserPositionDto } from '../../../../shared/api/maps';
import { LocationMap, MapLocation } from '../../../../shared/map/location-map/location-map';
import { serverMessage } from '../../../../shared/util/server-message';
import { SocialChallengeExecutionDto } from '../../api/games-api-types';
import { SocialChallengePlaying } from '../social-challenge-playing';

const REPORT_INTERVAL_MS = 10_000;

@Component({
  imports: [LocationMap, RouterLink],
  selector: 'app-play-social-challenge',
  styleUrl: './play-social-challenge.scss',
  templateUrl: './play-social-challenge.html',
})
export class PlaySocialChallenge {
  private readonly socialChallengePlaying = inject(SocialChallengePlaying);

  readonly id = input.required<string>();

  protected readonly execution = httpResource<SocialChallengeExecutionDto>(
    () => `/api/games/social-challenges/${this.id()}/execution`,
  );

  protected readonly executionErrorStatus = computed(() => {
    const failure = this.execution.error();
    return failure instanceof HttpErrorResponse ? failure.status : null;
  });

  // The explorer's saved position. While it loads or when reading fails, the map draws no pin.
  protected readonly userPosition = httpResource<UserPositionDto>(() => '/api/maps/user-positions/me');

  protected readonly position = computed<MapLocation | null>(() => {
    if (!this.userPosition.hasValue()) {
      return null;
    }
    const position = this.userPosition.value();
    return { latitude: position.latitude, longitude: position.longitude };
  });

  // Only 404 means the explorer has not set a position yet; 401 or 500 do not.
  protected readonly positionMissing = computed(() => {
    const failure = this.userPosition.error();
    return failure instanceof HttpErrorResponse && failure.status === 404;
  });

  protected readonly positionError = signal<string | null>(null);

  // Every 10 seconds the page reports the explorer's position while the execution is active,
  // then reads the execution again to show the new count. The timer stops when the explorer leaves the page.
  constructor() {
    const timer = setInterval(() => {
      if (this.execution.value()?.status === 'Active') {
        void this.report();
      }
    }, REPORT_INTERVAL_MS);
    inject(DestroyRef).onDestroy(() => clearInterval(timer));
  }

  // The pin moves only once the server has saved the position and the page has read it again.
  protected async setPosition(position: MapLocation): Promise<void> {
    this.positionError.set(null);
    try {
      await this.socialChallengePlaying.setPosition(position);
      this.userPosition.reload();
    } catch (failure) {
      this.positionError.set(serverMessage(failure, 'Could not save your position.'));
    }
  }

  // Every report first sends the explorer's current position again, so the server sees a fresh one
  // even when the explorer has not moved. A report the server refuses (an expired execution, a stale
  // position) changes nothing on the page by itself: the reloaded execution shows the state.
  private async report(): Promise<void> {
    const position = this.position();
    try {
      if (position !== null) {
        await this.socialChallengePlaying.setPosition(position);
      }
      await this.socialChallengePlaying.reportPosition(this.id());
    } catch {
      // The reload below shows the outcome.
    } finally {
      this.execution.reload();
    }
  }
}
