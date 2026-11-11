import { Component, inject, signal } from '@angular/core';
import { ImagePicker } from '../../shared/images/image-picker/image-picker';
import { ImageUploading } from '../../shared/images/image-uploading';
import { serverMessage } from '../../shared/util/server-message';

@Component({
  imports: [ImagePicker],
  selector: 'app-images',
  styleUrl: './images.scss',
  templateUrl: './images.html',
})
export class Images {
  private readonly imageUploading = inject(ImageUploading);

  protected readonly selected = signal<File | null>(null);
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly address = signal<string | null>(null);
  protected readonly imageMissing = signal(false);

  protected select(file: File | null): void {
    this.error.set(null);
    this.address.set(null);
    this.selected.set(file);
  }

  protected async send(): Promise<void> {
    const file = this.selected();
    if (file === null) {
      return;
    }
    this.pending.set(true);
    this.error.set(null);
    try {
      const saved = await this.imageUploading.upload(file);
      this.imageMissing.set(false);
      this.address.set(saved.address);
      this.selected.set(null);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not upload the image.'));
    } finally {
      this.pending.set(false);
    }
  }
}
