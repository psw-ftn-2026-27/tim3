import { Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { CreateSocialChallengeDto } from '../../api/games-api-types';
import { SocialChallengeAuthoring } from '../social-challenge-authoring';
import { SocialChallengeForm } from '../social-challenge-form/social-challenge-form';

@Component({
  imports: [SocialChallengeForm],
  selector: 'app-create-social-challenge',
  styleUrl: './create-social-challenge.scss',
  templateUrl: './create-social-challenge.html',
})
export class CreateSocialChallenge {
  private readonly socialChallengeAuthoring = inject(SocialChallengeAuthoring);
  private readonly router = inject(Router);

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async create(dto: CreateSocialChallengeDto): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.socialChallengeAuthoring.create(dto);
      await this.router.navigate(['/games/challenges']);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Izazov ne može da se sačuva.'));
    } finally {
      this.pending.set(false);
    }
  }
}
