import { HttpErrorResponse, httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { FormField, form, required } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { PageResult } from '../../../../shared/api/page-result';
import { ImagePicker } from '../../../../shared/images/image-picker/image-picker';
import { ImageUploading } from '../../../../shared/images/image-uploading';
import { serverMessage } from '../../../../shared/util/server-message';
import { ClubDto } from '../../api/social-api-types';
import { ClubCard } from '../club-card/club-card';
import { ClubManagement } from '../club-management';

@Component({
  imports: [ClubCard, FormField, ImagePicker, RouterLink],
  selector: 'app-club-list',
  styleUrl: './club-list.scss',
  templateUrl: './club-list.html',
})
export class ClubList {
  private readonly clubManagement = inject(ClubManagement);
  private readonly imageUploading = inject(ImageUploading);
  protected readonly auth = inject(Auth);

  protected readonly clubs = httpResource<PageResult<ClubDto>>(
    () => '/api/social/clubs?page=1&pageSize=20',
  );

  protected readonly isExplorer = computed(
    () => this.auth.user()?.roles.includes('explorer') ?? false,
  );

  protected readonly selectedClubId = signal<string | null>(null);
  protected readonly selectedImage = signal<File | null>(null);
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly formError = signal<string | null>(null);
  protected readonly imageError = signal<string | null>(null);

  protected readonly form = form(
    signal({ name: '', description: '', images: [] as string[] }),
    (path) => {
      required(path.name, { message: 'Name is required.' });
      required(path.description, { message: 'Description is required.' });
    },
  );

  protected select(club: ClubDto): void {
    this.error.set(null);
    this.formError.set(null);
    this.imageError.set(null);
    this.selectedImage.set(null);
    this.selectedClubId.set(club.id);
    this.form().reset({ name: club.name, description: club.description, images: club.images });
  }

  protected cancelEdit(): void {
    this.selectedClubId.set(null);
  }

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

  protected async update(event: Event): Promise<void> {
    event.preventDefault();
    const clubId = this.selectedClubId();
    if (clubId === null) {
      return;
    }
    this.pending.set(true);
    this.error.set(null);
    this.formError.set(null);
    try {
      await this.clubManagement.update(clubId, {
        name: this.form.name().value(),
        description: this.form.description().value(),
        images: this.form.images().value(),
      });
      this.selectedClubId.set(null);
      this.clubs.reload();
    } catch (failure) {
      if (failure instanceof HttpErrorResponse && failure.status === 404) {
        this.selectedClubId.set(null);
        this.error.set(serverMessage(failure, 'Could not update the club.'));
      } else {
        this.formError.set(serverMessage(failure, 'Could not update the club.'));
      }
    } finally {
      this.pending.set(false);
    }
  }

  protected async remove(clubId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.clubManagement.delete(clubId);
      if (this.selectedClubId() === clubId) {
        this.selectedClubId.set(null);
      }
      this.clubs.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not delete the club.'));
    } finally {
      this.pending.set(false);
    }
  }
}
