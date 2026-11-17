import { HttpErrorResponse, httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { FormField, form, min, required, validate } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { TourDto, TransportMode } from '../../api/exploration-api-types';
import { TourAuthoring } from '../tour-authoring';

@Component({
  imports: [FormField, RouterLink],
  selector: 'app-my-tours',
  styleUrl: './my-tours.scss',
  templateUrl: './my-tours.html',
})
export class MyTours {
  private readonly tourAuthoring = inject(TourAuthoring);

  protected readonly tours = httpResource<TourDto[]>(() => '/api/exploration/tours/mine');

  protected readonly toursErrorStatus = computed(() => {
    const failure = this.tours.error();
    return failure instanceof HttpErrorResponse ? failure.status : null;
  });

  protected readonly selectedTourId = signal<string | null>(null);
  protected readonly pending = signal(false);
  // A failed row command (archive, reactivate) shows under its row, a failed form command
  // inside its form. A saved form closes, so its success shows below the table instead.
  protected readonly rowError = signal<{ rowId: string; message: string } | null>(null);
  protected readonly formError = signal<string | null>(null);
  protected readonly success = signal<string | null>(null);

  private readonly model = signal({ transport: 'Walking' as TransportMode, minutes: 30 });

  protected readonly form = form(this.model, (path) => {
    required(path.minutes, { message: 'Minutes are required.' });
    min(path.minutes, 1, { message: 'Minutes must be at least 1.' });
  });

  protected readonly publishTourId = signal<string | null>(null);

  private readonly publishModel = signal({
    price: '' as '' | 'Free' | 'Paid',
    amount: null as number | null,
  });

  protected readonly publishForm = form(this.publishModel, (path) => {
    required(path.price);
    required(path.amount, {
      message: 'Amount is required.',
      when: ({ valueOf }) => valueOf(path.price) === 'Paid',
    });
    validate(path.amount, ({ value, valueOf }) => {
      const amount = value();
      return valueOf(path.price) !== 'Paid' || amount === null || amount > 0
        ? null
        : { kind: 'positive', message: 'Amount must be greater than zero.' };
    });
  });

  protected select(tourId: string): void {
    this.clearMessages();
    this.publishTourId.set(null);
    this.selectedTourId.set(tourId);
    this.form().reset({ transport: 'Walking', minutes: 30 });
  }

  protected selectForPublish(tourId: string): void {
    this.clearMessages();
    this.selectedTourId.set(null);
    this.publishTourId.set(tourId);
    this.publishForm().reset({ price: '', amount: null });
  }

  protected async publish(event: Event): Promise<void> {
    event.preventDefault();
    const tourId = this.publishTourId();
    if (tourId === null || !this.publishForm().valid()) {
      return;
    }
    const isFree = this.publishForm.price().value() === 'Free';
    this.pending.set(true);
    this.clearMessages();
    try {
      await this.tourAuthoring.publish(tourId, {
        isFree,
        amount: isFree ? null : this.publishForm.amount().value(),
      });
      this.tours.reload();
      this.publishTourId.set(null);
      this.success.set('Tura je objavljena.');
    } catch (failure) {
      this.formError.set(serverMessage(failure, 'Could not publish the tour.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async archive(tourId: string): Promise<void> {
    this.pending.set(true);
    this.clearMessages();
    try {
      await this.tourAuthoring.archive(tourId);
      this.tours.reload();
    } catch (failure) {
      this.rowError.set({
        rowId: tourId,
        message: serverMessage(failure, 'Could not archive the tour.'),
      });
    } finally {
      this.pending.set(false);
    }
  }

  protected async reactivate(tourId: string): Promise<void> {
    this.pending.set(true);
    this.clearMessages();
    try {
      await this.tourAuthoring.reactivate(tourId);
      this.tours.reload();
    } catch (failure) {
      this.rowError.set({
        rowId: tourId,
        message: serverMessage(failure, 'Could not reactivate the tour.'),
      });
    } finally {
      this.pending.set(false);
    }
  }

  protected archivedAt(iso: string): string {
    return new Date(iso).toLocaleString('sr-RS');
  }

  protected async addTransportTime(event: Event): Promise<void> {
    event.preventDefault();
    const tourId = this.selectedTourId();
    if (tourId === null) {
      return;
    }
    this.pending.set(true);
    this.clearMessages();
    try {
      await this.tourAuthoring.addTransportTime(tourId, {
        transport: this.form.transport().value(),
        minutes: this.form.minutes().value(),
      });
      this.tours.reload();
      this.selectedTourId.set(null);
      this.success.set('Vreme prevoza je dodato.');
    } catch (failure) {
      this.formError.set(serverMessage(failure, 'Could not add the transport time.'));
    } finally {
      this.pending.set(false);
    }
  }

  // The next command, or opening a form, clears the messages of the previous one.
  private clearMessages(): void {
    this.rowError.set(null);
    this.formError.set(null);
    this.success.set(null);
  }
}
