import { httpResource } from '@angular/common/http';
import { Component, effect, inject, signal } from '@angular/core';
import { FormField, form } from '@angular/forms/signals';
import { serverMessage } from '../../../../shared/util/server-message';
import { TourDifficulty, TourPreferenceDto, TransportMode } from '../../api/exploration-api-types';
import { TourPreferenceManagement } from '../tour-preference-management';

const EMPTY_MODEL = {
  easy: false,
  moderate: false,
  hard: false,
  transport: '' as TransportMode | '',
  tags: '',
  appliesToSearch: false,
};

@Component({
  imports: [FormField],
  selector: 'app-my-tour-preference',
  styleUrl: './my-tour-preference.scss',
  templateUrl: './my-tour-preference.html',
})
export class MyTourPreference {
  private readonly tourPreferenceManagement = inject(TourPreferenceManagement);

  protected readonly preference = httpResource<TourPreferenceDto | null>(
    () => '/api/exploration/tour-preferences/mine',
  );

  protected readonly form = form(signal({ ...EMPTY_MODEL }));

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  constructor() {
    effect(() => {
      // Fill the form with the saved preference every time it arrives from the server.
      const preference = this.preference.value();
      if (preference === undefined) {
        return;
      }
      if (preference === null) {
        this.form().reset({ ...EMPTY_MODEL });
        return;
      }
      this.form().reset({
        easy: preference.difficulties.includes('Easy'),
        moderate: preference.difficulties.includes('Moderate'),
        hard: preference.difficulties.includes('Hard'),
        transport: preference.transport ?? '',
        tags: preference.tags.join(', '),
        appliesToSearch: preference.appliesToSearch,
      });
    });
  }

  protected async save(event: Event): Promise<void> {
    event.preventDefault();
    this.pending.set(true);
    this.error.set(null);
    try {
      const transport = this.form.transport().value();
      await this.tourPreferenceManagement.save({
        difficulties: this.chosenDifficulties(),
        transport: transport === '' ? null : transport,
        tags: this.form
          .tags()
          .value()
          .split(',')
          .map((tag) => tag.trim())
          .filter((tag) => tag !== ''),
        appliesToSearch: this.form.appliesToSearch().value(),
      });
      this.preference.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not save the preference.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async remove(): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.tourPreferenceManagement.remove();
      this.preference.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not delete the preference.'));
    } finally {
      this.pending.set(false);
    }
  }

  private chosenDifficulties(): TourDifficulty[] {
    const difficulties: TourDifficulty[] = [];
    if (this.form.easy().value()) {
      difficulties.push('Easy');
    }
    if (this.form.moderate().value()) {
      difficulties.push('Moderate');
    }
    if (this.form.hard().value()) {
      difficulties.push('Hard');
    }
    return difficulties;
  }
}
