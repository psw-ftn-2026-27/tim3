import { httpResource } from '@angular/common/http';
import { Component, inject, input, signal } from '@angular/core';
import { Router } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { SocialChallengeDto, UpdateSocialChallengeDto } from '../../api/games-api-types';
import { SocialChallengeAuthoring } from '../social-challenge-authoring';
import {
  SocialChallengeForm,
  SocialChallengeFormValue,
} from '../social-challenge-form/social-challenge-form';

@Component({
  imports: [SocialChallengeForm],
  selector: 'app-edit-social-challenge',
  styleUrl: './edit-social-challenge.scss',
  templateUrl: './edit-social-challenge.html',
})
export class EditSocialChallenge {
  private readonly socialChallengeAuthoring = inject(SocialChallengeAuthoring);
  private readonly router = inject(Router);

  readonly id = input.required<string>();

  protected readonly challenge = httpResource<SocialChallengeDto>(
    () => `/api/games/social-challenges/${this.id()}`,
  );

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  // The server sends times as "HH:mm:ss"; the time inputs take "HH:mm".
  protected toFormValue(challenge: SocialChallengeDto): SocialChallengeFormValue {
    return {
      name: challenge.name,
      activePeriodStart: challenge.activePeriodStart.slice(0, 5),
      activePeriodEnd: challenge.activePeriodEnd.slice(0, 5),
      peopleRequired: challenge.peopleRequired,
      gatheringRadius: challenge.gatheringRadius,
      experienceGained: challenge.experienceGained,
      endMessage: challenge.endMessage,
    };
  }

  protected async update(dto: UpdateSocialChallengeDto): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.socialChallengeAuthoring.update(this.id(), dto);
      await this.router.navigate(['/games/challenges']);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Izazov ne može da se sačuva.'));
    } finally {
      this.pending.set(false);
    }
  }
}
