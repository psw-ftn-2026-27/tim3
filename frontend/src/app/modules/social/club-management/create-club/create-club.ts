import { Component, inject, signal } from '@angular/core';
import { FormField, form, required } from '@angular/forms/signals';
import { Router } from '@angular/router';
import { ImagePicker } from '../../../../shared/images/image-picker/image-picker';
import { ImageUploading } from '../../../../shared/images/image-uploading';
import { serverMessage } from '../../../../shared/util/server-message';
import { ClubManagement } from '../club-management';

@Component({
  imports: [FormField, ImagePicker],
  selector: 'app-create-club',
  styleUrl: './create-club.scss',
  templateUrl: './create-club.html',
})
export class CreateClub {
  private readonly clubManagement = inject(ClubManagement);
  private readonly imageUploading = inject(ImageUploading);
  private readonly router = inject(Router);

  protected readonly form = form(
    signal({ name: '', description: '', images: [] as string[] }),
    (path) => {
      required(path.name, { message: 'Name is required.' });
      required(path.description, { message: 'Description is required.' });
    },
  );

  protected readonly selectedImage = signal<File | null>(null);
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly imageError = signal<string | null>(null);

  protected selectImage(file: File | null): void {
    this.imageError.set(null);
    this.selectedImage.set(file);
  }

  protected async sendImage(): Promise<void> {
    const file = this.selectedImage();
    if (file === null) {
      return;
    }
    this.pending.set(true);
    this.imageError.set(null);
    try {
      const saved = await this.imageUploading.upload(file);
      this.form.images().value.update((images) => [...images, saved.address]);
      this.selectedImage.set(null);
    } catch (failure) {
      this.imageError.set(serverMessage(failure, 'Could not upload the image.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected removeImage(index: number): void {
    this.form.images().value.update((images) => images.filter((_, i) => i !== index));
  }

  protected async submit(event: Event): Promise<void> {
    event.preventDefault();
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.clubManagement.create({
        name: this.form.name().value(),
        description: this.form.description().value(),
        images: this.form.images().value(),
      });
      await this.router.navigate(['/social/clubs']);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not save the club.'));
    } finally {
      this.pending.set(false);
    }
  }
}
