import { HttpErrorResponse, httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { Auth } from '../../../core/auth/auth';
import { UserPositionDto } from '../../../shared/api/maps';
import { PageResult } from '../../../shared/api/page-result';
import { LocationMap, MapLocation } from '../../../shared/map/location-map/location-map';
import { PublishedQuizDto, PublishedSocialChallengeDto } from '../api/games-api-types';
import { commandFailure, QuizPlaying } from '../quiz-playing/quiz-playing';
import { SocialChallengePlaying } from '../social-challenge-playing/social-challenge-playing';

@Component({
  imports: [LocationMap, RouterLink],
  selector: 'app-games-home',
  styleUrl: './games-home.scss',
  templateUrl: './games-home.html',
})
export class GamesHome {
  private readonly auth = inject(Auth);
  private readonly quizPlaying = inject(QuizPlaying);
  private readonly socialChallengePlaying = inject(SocialChallengePlaying);
  private readonly router = inject(Router);

  // The tab the user is looking at; it starts on quizzes on every visit.
  protected readonly tab = signal<'quizzes' | 'social' | 'location'>('quizzes');

  protected readonly isAdministrator = computed(
    () => this.auth.user()?.roles.includes('administrator') ?? false,
  );

  protected readonly isExplorer = computed(
    () => this.auth.user()?.roles.includes('explorer') ?? false,
  );

  // Only explorers play quizzes, so the published quizzes and the position are read only for them.
  protected readonly quizzes = httpResource<PageResult<PublishedQuizDto>>(() =>
    this.isExplorer() ? '/api/games/quizzes/published?page=1&pageSize=20' : undefined,
  );

  protected readonly quizzesErrorStatus = computed(() => {
    const failure = this.quizzes.error();
    return failure instanceof HttpErrorResponse ? failure.status : null;
  });

  protected readonly quizList = computed(() => this.quizzes.value()?.items ?? []);

  protected readonly quizLocations = computed<MapLocation[]>(() =>
    this.quizList().map((quiz) => ({
      latitude: quiz.location.latitude,
      longitude: quiz.location.longitude,
    })),
  );

  // Only explorers play social challenges, so the published challenges are read only for them.
  protected readonly socialChallenges = httpResource<PageResult<PublishedSocialChallengeDto>>(() =>
    this.isExplorer() ? '/api/games/social-challenges/published?page=1&pageSize=20' : undefined,
  );

  protected readonly socialChallengesErrorStatus = computed(() => {
    const failure = this.socialChallenges.error();
    return failure instanceof HttpErrorResponse ? failure.status : null;
  });

  protected readonly socialChallengeList = computed(() => this.socialChallenges.value()?.items ?? []);

  protected readonly socialChallengeLocations = computed<MapLocation[]>(() =>
    this.socialChallengeList().map((challenge) => ({
      latitude: challenge.latitude,
      longitude: challenge.longitude,
    })),
  );

  // The explorer's saved position. While it loads or when reading fails, the map draws no pin.
  protected readonly userPosition = httpResource<UserPositionDto>(() =>
    this.isExplorer() ? '/api/maps/user-positions/me' : undefined,
  );

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

  // The map is folded away on every visit, so the quizzes are the first thing the explorer sees.
  protected readonly mapOpen = signal(false);

  protected readonly pending = signal(false);
  // The failed activation, shown on the card of the quiz the explorer tried to start.
  protected readonly startError = signal<{ quizId: string; message: string } | null>(null);
  // The failed activation, shown on the card of the social challenge the explorer tried to activate.
  protected readonly activateError = signal<{ challengeId: string; message: string } | null>(null);

  // The pin moves only once the server has saved the position and the page has read it again.
  protected async setPosition(position: MapLocation): Promise<void> {
    this.positionError.set(null);
    try {
      await this.quizPlaying.setPosition(position);
      this.userPosition.reload();
    } catch (error) {
      this.positionError.set(commandFailure(error));
    }
  }

  protected async start(quizId: string): Promise<void> {
    this.pending.set(true);
    this.startError.set(null);
    try {
      await this.quizPlaying.activate(quizId);
      await this.router.navigate(['/games/quizzes', quizId, 'play']);
    } catch (error) {
      this.startError.set({ quizId, message: commandFailure(error) });
    } finally {
      this.pending.set(false);
    }
  }

  protected async activate(challengeId: string): Promise<void> {
    this.pending.set(true);
    this.activateError.set(null);
    try {
      await this.socialChallengePlaying.activate(challengeId);
      await this.router.navigate(['/games/challenges', challengeId, 'play']);
    } catch (error) {
      this.activateError.set({ challengeId, message: commandFailure(error) });
    } finally {
      this.pending.set(false);
    }
  }
}
