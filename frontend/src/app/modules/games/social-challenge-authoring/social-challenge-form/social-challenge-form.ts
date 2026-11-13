import { Component, effect, input, output, signal } from '@angular/core';
import { FormField, form, max, min, required } from '@angular/forms/signals';
import { CreateSocialChallengeDto } from '../../api/games-api-types';

export interface SocialChallengeFormValue {
  name: string;
  activePeriodStart: string;
  activePeriodEnd: string;
  peopleRequired: number;
  gatheringRadius: number;
  experienceGained: number;
  endMessage: string;
}

@Component({
  imports: [FormField],
  selector: 'app-social-challenge-form',
  styleUrl: './social-challenge-form.scss',
  templateUrl: './social-challenge-form.html',
})
export class SocialChallengeForm {
  readonly initial = input<SocialChallengeFormValue | null>(null);
  readonly pending = input(false);
  readonly error = input<string | null>(null);

  readonly save = output<CreateSocialChallengeDto>();

  protected readonly form = form(
    signal<SocialChallengeFormValue>({
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

  private prefilled = false;

  constructor() {
    effect(() => {
      // Seed the form once with the values the edit page passes in.
      const initial = this.initial();
      if (initial && !this.prefilled) {
        this.prefilled = true;
        this.form().reset(initial);
      }
    });
  }

  protected submitForm(event: Event): void {
    event.preventDefault();
    // The time inputs give "HH:mm"; the server expects "HH:mm:ss".
    this.save.emit({
      name: this.form.name().value(),
      activePeriodStart: `${this.form.activePeriodStart().value()}:00`,
      activePeriodEnd: `${this.form.activePeriodEnd().value()}:00`,
      peopleRequired: this.form.peopleRequired().value(),
      gatheringRadius: this.form.gatheringRadius().value(),
      experienceGained: this.form.experienceGained().value(),
      endMessage: this.form.endMessage().value(),
    });
  }
}
