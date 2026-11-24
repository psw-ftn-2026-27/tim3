import { Component, inject, signal } from '@angular/core';
import { FormField, form, min, required, validate } from '@angular/forms/signals';
import { Router } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { TourDifficulty } from '../../api/exploration-api-types';
import { TourAuthoring } from '../tour-authoring';

@Component({
  imports: [FormField],
  selector: 'app-create-tour',
  styleUrl: './create-tour.scss',
  templateUrl: './create-tour.html',
})
export class CreateTour {
  private readonly tourAuthoring = inject(TourAuthoring);
  private readonly router = inject(Router);

  protected readonly form = form(
    signal({
      name: '',
      description: '',
      difficulty: 'Easy' as TourDifficulty,
      tags: '',
      groupSizeFrom: null as number | null,
      groupSizeTo: null as number | null,
    }),
    (path) => {
      required(path.name, { message: 'Name is required.' });
      required(path.description, { message: 'Description is required.' });
      required(path.groupSizeFrom, { message: 'From is required.' });
      min(path.groupSizeFrom, 1, { message: 'From must be at least 1.' });
      validate(path.groupSizeFrom, ({ value }) => {
        const from = value();
        return from === null || Number.isInteger(from)
          ? null
          : { kind: 'integer', message: 'From must be a whole number.' };
      });
      required(path.groupSizeTo, { message: 'To is required.' });
      validate(path.groupSizeTo, ({ value }) => {
        const to = value();
        return to === null || Number.isInteger(to)
          ? null
          : { kind: 'integer', message: 'To must be a whole number.' };
      });
      validate(path.groupSizeTo, ({ value, valueOf }) => {
        const from = valueOf(path.groupSizeFrom);
        const to = value();
        return from === null || to === null || to >= from
          ? null
          : { kind: 'range', message: 'To must be greater than or equal to From.' };
      });
    },
  );

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async submit(event: Event): Promise<void> {
    event.preventDefault();
    const from = this.form.groupSizeFrom().value();
    const to = this.form.groupSizeTo().value();
    if (from === null || to === null || !this.form().valid()) {
      return;
    }
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.tourAuthoring.create({
        name: this.form.name().value(),
        description: this.form.description().value(),
        difficulty: this.form.difficulty().value(),
        tags: this.form
          .tags()
          .value()
          .split(',')
          .map((tag) => tag.trim())
          .filter((tag) => tag !== ''),
        recommendedGroupSize: { from, to },
      });
      await this.router.navigate(['/exploration/mine']);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not create the tour.'));
    } finally {
      this.pending.set(false);
    }
  }
}
