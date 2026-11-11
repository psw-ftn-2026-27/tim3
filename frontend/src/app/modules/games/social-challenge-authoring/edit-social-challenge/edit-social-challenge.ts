import { httpResource } from '@angular/common/http';
import { Component, effect, inject, input, signal } from '@angular/core';
import { FormField, form, max, min, required } from '@angular/forms/signals';
import { Router } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { SocialChallengeDto } from '../../api/games-api-types';
import { SocialChallengeAuthoring } from '../social-challenge-authoring';

@Component({
  imports: [FormField],
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

  private prefilled = false;

  constructor() {
    effect(() => {
      // Seed the editable form once the challenge arrives from the server.
      // The server sends times as "HH:mm:ss"; the time input takes "HH:mm".
      const challenge = this.challenge.value();
      if (challenge && !this.prefilled) {
        this.prefilled = true;
        this.form().reset({
          name: challenge.name,
          activePeriodStart: challenge.activePeriodStart.slice(0, 5),
          activePeriodEnd: challenge.activePeriodEnd.slice(0, 5),
          peopleRequired: challenge.peopleRequired,
          gatheringRadius: challenge.gatheringRadius,
          experienceGained: challenge.experienceGained,
          endMessage: challenge.endMessage,
        });
      }
    });
  }

  protected async submit(event: Event): Promise<void> {
    event.preventDefault();
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.socialChallengeAuthoring.update(this.id(), {
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
