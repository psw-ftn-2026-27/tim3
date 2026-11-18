import { HttpErrorResponse, httpResource } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { ImageUploading } from '../../../../shared/images/image-uploading';
import { serverMessage } from '../../../../shared/util/server-message';
import { LocationChallengeDto, UpdateLocationChallengeDto } from '../../api/games-api-types';
import { LocationChallengeAuthoring } from '../location-challenge-authoring';
import {
  LocationChallengeForm,
  LocationChallengeFormValue,
} from '../location-challenge-form/location-challenge-form';

@Component({
  imports: [RouterLink, LocationChallengeForm],
  selector: 'app-edit-location-challenge',
  styleUrl: './edit-location-challenge.scss',
  templateUrl: './edit-location-challenge.html',
})
export class EditLocationChallenge {
  private readonly locationChallengeAuthoring = inject(LocationChallengeAuthoring);
  private readonly imageUploading = inject(ImageUploading);
  private readonly router = inject(Router);

  readonly id = input.required<string>();

  protected readonly challenge = httpResource<LocationChallengeDto>(
    () => `/api/games/location-challenges/${this.id()}`,
  );

  protected readonly challengeErrorStatus = computed(() => {
    const failure = this.challenge.error();
    return failure instanceof HttpErrorResponse ? failure.status : null;
  });

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  // The photo of the point the form is adding.
  protected readonly photoAddress = signal<string | null>(null);
  protected readonly photoPending = signal(false);
  protected readonly photoError = signal<string | null>(null);

  // Deleting cannot be undone, so the page first asks for confirmation.
  protected readonly confirmingDelete = signal(false);
  protected readonly deleteError = signal<string | null>(null);

  protected toFormValue(challenge: LocationChallengeDto): LocationChallengeFormValue {
    return {
      name: challenge.name,
      experienceGained: challenge.experienceGained,
      startLocation: { latitude: challenge.startLatitude, longitude: challenge.startLongitude },
      hiddenPoints: challenge.hiddenPoints.map((point) => ({
        photo: point.photo,
        latitude: point.latitude,
        longitude: point.longitude,
      })),
    };
  }

  protected async uploadPhoto(file: File): Promise<void> {
    this.photoPending.set(true);
    this.photoError.set(null);
    try {
      const saved = await this.imageUploading.upload(file);
      this.photoAddress.set(saved.address);
    } catch (failure) {
      this.photoError.set(serverMessage(failure, 'Could not upload the photo.'));
    } finally {
      this.photoPending.set(false);
    }
  }

  protected async update(dto: UpdateLocationChallengeDto): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.locationChallengeAuthoring.update(this.id(), dto);
      await this.router.navigate(['/games/location-challenges']);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not save the challenge.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected askToDelete(): void {
    this.deleteError.set(null);
    this.confirmingDelete.set(true);
  }

  protected async confirmDelete(): Promise<void> {
    this.confirmingDelete.set(false);
    this.pending.set(true);
    this.deleteError.set(null);
    try {
      await this.locationChallengeAuthoring.delete(this.id());
      await this.router.navigate(['/games/location-challenges']);
    } catch (failure) {
      this.deleteError.set(serverMessage(failure, 'Could not delete the challenge.'));
    } finally {
      this.pending.set(false);
    }
  }
}
