import { Component, computed, input, output, signal } from '@angular/core';

const MAX_SIZE_BYTES = 5 * 1024 * 1024;
const ALLOWED_TYPES = ['image/jpeg', 'image/png', 'image/webp'];

let nextInputNumber = 0;

@Component({
  selector: 'app-image-picker',
  styleUrl: './image-picker.scss',
  templateUrl: './image-picker.html',
})
export class ImagePicker {
  readonly file = input<File | null>(null);
  readonly pending = input(false);

  readonly fileChange = output<File | null>();
  readonly send = output<void>();

  protected readonly inputId = `image-picker-${nextInputNumber++}`;
  protected readonly error = signal<string | null>(null);

  protected readonly previewUrl = computed(() => {
    const file = this.file();
    return file === null ? null : URL.createObjectURL(file);
  });

  protected select(event: Event): void {
    const element = event.target as HTMLInputElement;
    const file = element.files?.[0] ?? null;
    element.value = '';
    if (file === null) {
      return;
    }
    if (!ALLOWED_TYPES.includes(file.type)) {
      this.error.set('Unsupported format. Choose a JPEG, PNG or WebP image.');
      this.fileChange.emit(null);
      return;
    }
    if (file.size > MAX_SIZE_BYTES) {
      this.error.set('The image is too large. The maximum size is 5 MB.');
      this.fileChange.emit(null);
      return;
    }
    this.error.set(null);
    this.fileChange.emit(file);
  }
}
