import { Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { ImageUploading } from '../../../../shared/images/image-uploading';
import { serverMessage } from '../../../../shared/util/server-message';
import { CreateLocationChallengeDto } from '../../api/games-api-types';
import { LocationChallengeAuthoring } from '../location-challenge-authoring';
import { LocationChallengeForm } from '../location-challenge-form/location-challenge-form';

@Component({
  imports: [LocationChallengeForm],
  selector: 'app-create-location-challenge',
  styleUrl: './create-location-challenge.scss',
  templateUrl: './create-location-challenge.html',
})
export class CreateLocationChallenge {
  private readonly locationChallengeAuthoring = inject(LocationChallengeAuthoring);
  private readonly imageUploading = inject(ImageUploading);
  private readonly router = inject(Router);

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  // The photo of the point the form is adding.
  protected readonly photoAddress = signal<string | null>(null);
  protected readonly photoPending = signal(false);
  protected readonly photoError = signal<string | null>(null);

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

  protected async create(dto: CreateLocationChallengeDto): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.locationChallengeAuthoring.create(dto);
      await this.router.navigate(['/games/location-challenges']);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not save the challenge.'));
    } finally {
      this.pending.set(false);
    }
  }
}
