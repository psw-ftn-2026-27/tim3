import { Component, computed, effect, input, output, signal } from '@angular/core';
import { FormField, form, max, min, required } from '@angular/forms/signals';
import { ImagePicker } from '../../../../shared/images/image-picker/image-picker';
import { LocationMap, MapLocation } from '../../../../shared/map/location-map/location-map';
import { CreateHiddenPointDto, CreateLocationChallengeDto } from '../../api/games-api-types';

export interface LocationChallengeFormValue {
  name: string;
  experienceGained: number;
  startLocation: MapLocation | null;
  hiddenPoints: CreateHiddenPointDto[];
}

@Component({
  imports: [FormField, ImagePicker, LocationMap],
  selector: 'app-location-challenge-form',
  styleUrl: './location-challenge-form.scss',
  templateUrl: './location-challenge-form.html',
})
export class LocationChallengeForm {
  readonly initial = input<LocationChallengeFormValue | null>(null);
  readonly pending = input(false);
  readonly error = input<string | null>(null);
  // The page uploads the photo of the new point and passes its address back here.
  readonly photoAddress = input<string | null>(null);
  readonly photoPending = input(false);
  readonly photoError = input<string | null>(null);

  readonly save = output<CreateLocationChallengeDto>();
  readonly photoSend = output<File>();
  // Raised once the uploaded photo belongs to an added point, so the page forgets its address.
  readonly photoUsed = output<void>();

  protected readonly form = form(
    signal({ name: '', experienceGained: 1 }),
    (path) => {
      required(path.name, { message: 'Name is required.' });
      required(path.experienceGained, { message: 'XP is required.' });
      min(path.experienceGained, 1, { message: 'XP must be at least 1.' });
      max(path.experienceGained, 7000, { message: 'XP must be at most 7000.' });
    },
  );

  // What a click on the map does: pick the start location, pick the pin of the new point,
  // or nothing, while the map shows the start location and all hidden points.
  protected readonly mode = signal<'start' | 'point' | 'overview'>('start');
  protected readonly startLocation = signal<MapLocation | null>(null);
  protected readonly hiddenPoints = signal<CreateHiddenPointDto[]>([]);

  // The point being added: its photo file before upload and its pin.
  protected readonly selectedPhoto = signal<File | null>(null);
  protected readonly newPin = signal<MapLocation | null>(null);

  protected readonly mapLocations = computed<MapLocation[]>(() => {
    const start = this.startLocation();
    const pin = this.newPin();
    switch (this.mode()) {
      case 'start':
        return start === null ? [] : [start];
      case 'point':
        return pin === null ? [] : [pin];
      case 'overview':
        return [
          ...(start === null ? [] : [start]),
          ...this.hiddenPoints().map((point) => ({
            latitude: point.latitude,
            longitude: point.longitude,
          })),
        ];
    }
  });

  protected readonly canAddPoint = computed(
    () => this.photoAddress() !== null && this.newPin() !== null,
  );

  protected readonly canSave = computed(
    () =>
      this.form().valid() &&
      this.startLocation() !== null &&
      this.hiddenPoints().length > 0 &&
      !this.pending(),
  );

  private prefilled = false;

  constructor() {
    effect(() => {
      // Seed the form once with the values the edit page passes in.
      const initial = this.initial();
      if (initial && !this.prefilled) {
        this.prefilled = true;
        this.form().reset({ name: initial.name, experienceGained: initial.experienceGained });
        this.startLocation.set(initial.startLocation);
        this.hiddenPoints.set(initial.hiddenPoints);
        this.mode.set('overview');
      }
    });
  }

  // With one pin allowed, the map reports either that pin or, after a click on it, nothing.
  protected changeMapLocations(locations: MapLocation[]): void {
    const location = locations[0] ?? null;
    if (this.mode() === 'start') {
      this.startLocation.set(location);
    } else if (this.mode() === 'point') {
      this.newPin.set(location);
    }
  }

  protected sendPhoto(): void {
    const file = this.selectedPhoto();
    if (file !== null) {
      this.photoSend.emit(file);
    }
  }

  protected addPoint(): void {
    const photo = this.photoAddress();
    const pin = this.newPin();
    if (photo === null || pin === null) {
      return;
    }
    this.hiddenPoints.update((points) => [
      ...points,
      { photo, latitude: pin.latitude, longitude: pin.longitude },
    ]);
    this.newPin.set(null);
    this.selectedPhoto.set(null);
    this.photoUsed.emit();
  }

  protected removePoint(index: number): void {
    this.hiddenPoints.update((points) => points.filter((_, i) => i !== index));
  }

  protected submitForm(event: Event): void {
    event.preventDefault();
    const start = this.startLocation();
    if (start === null) {
      return;
    }
    this.save.emit({
      name: this.form.name().value(),
      startLatitude: start.latitude,
      startLongitude: start.longitude,
      experienceGained: this.form.experienceGained().value(),
      hiddenPoints: this.hiddenPoints(),
    });
  }
}
