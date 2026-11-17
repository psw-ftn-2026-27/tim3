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
      required(path.name, { message: 'Name is required.' });
      required(path.activePeriodStart, { message: 'Start is required.' });
      required(path.activePeriodEnd, { message: 'End is required.' });
      required(path.peopleRequired, { message: 'Number of people is required.' });
      min(path.peopleRequired, 2, { message: 'At least 2 people are required.' });
      required(path.gatheringRadius, { message: 'Gathering radius is required.' });
      max(path.gatheringRadius, 50, { message: 'Gathering radius must be at most 50 m.' });
      required(path.experienceGained, { message: 'XP is required.' });
      min(path.experienceGained, 0, { message: 'XP must be at least 0.' });
      max(path.experienceGained, 20000, { message: 'XP must be at most 20000.' });
      required(path.endMessage, { message: 'End message is required.' });
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
