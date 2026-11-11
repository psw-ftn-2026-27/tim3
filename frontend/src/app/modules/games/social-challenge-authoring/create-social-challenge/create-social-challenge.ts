import { Component, inject, signal } from '@angular/core';
import { FormField, form, max, min, required } from '@angular/forms/signals';
import { Router } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { SocialChallengeAuthoring } from '../social-challenge-authoring';

@Component({
  imports: [FormField],
  selector: 'app-create-social-challenge',
  styleUrl: './create-social-challenge.scss',
  templateUrl: './create-social-challenge.html',
})
export class CreateSocialChallenge {
  private readonly socialChallengeAuthoring = inject(SocialChallengeAuthoring);
  private readonly router = inject(Router);

  protected readonly form = form(
    signal({
      name: '',
      activePeriodStart: '',
      activePeriodEnd: '',
      peopleRequired: 2,
      gatheringRadius: 10,
      experienceGained: 0,
      endMessage: '',
    }),
    (path) => {
      required(path.name, { message: 'Naziv je obavezan.' });
      required(path.activePeriodStart, { message: 'Početak je obavezan.' });
      required(path.activePeriodEnd, { message: 'Kraj je obavezan.' });
      required(path.peopleRequired, { message: 'Broj ljudi je obavezan.' });
      min(path.peopleRequired, 2, { message: 'Potrebno je najmanje 2 osobe.' });
      required(path.gatheringRadius, { message: 'Opseg je obavezan.' });
      max(path.gatheringRadius, 50, { message: 'Opseg je najviše 50 m.' });
      required(path.experienceGained, { message: 'XP je obavezan.' });
      min(path.experienceGained, 0, { message: 'XP je najmanje 0.' });
      max(path.experienceGained, 20000, { message: 'XP je najviše 20000.' });
      required(path.endMessage, { message: 'Izlazna poruka je obavezna.' });
    },
  );

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async submit(event: Event): Promise<void> {
    event.preventDefault();
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.socialChallengeAuthoring.create({
        name: this.form.name().value(),
        activePeriodStart: `${this.form.activePeriodStart().value()}:00`,
        activePeriodEnd: `${this.form.activePeriodEnd().value()}:00`,
        peopleRequired: this.form.peopleRequired().value(),
        gatheringRadius: this.form.gatheringRadius().value(),
        experienceGained: this.form.experienceGained().value(),
        endMessage: this.form.endMessage().value(),
      });
      await this.router.navigate(['/games/challenges']);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Izazov ne može da se sačuva.'));
    } finally {
      this.pending.set(false);
    }
  }
}
