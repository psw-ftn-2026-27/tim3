import { HttpErrorResponse, httpResource } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { FormField, form, required } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { ImagePicker } from '../../../../shared/images/image-picker/image-picker';
import { ImageUploading } from '../../../../shared/images/image-uploading';
import { LocationMap, MapLocation } from '../../../../shared/map/location-map/location-map';
import { serverMessage } from '../../../../shared/util/server-message';
import { KeyPointInputDto, TourKeyPointsDto } from '../../api/exploration-api-types';
import { TourAuthoring } from '../tour-authoring';

@Component({
  imports: [FormField, ImagePicker, LocationMap, RouterLink],
  selector: 'app-key-points',
  styleUrl: './key-points.scss',
  templateUrl: './key-points.html',
})
export class KeyPoints {
  private readonly tourAuthoring = inject(TourAuthoring);
  private readonly imageUploading = inject(ImageUploading);

  readonly id = input.required<string>();

  protected readonly tour = httpResource<TourKeyPointsDto>(
    () => `/api/exploration/tours/${this.id()}/key-points`,
  );

  protected readonly tourErrorStatus = computed(() => {
    const failure = this.tour.error();
    return failure instanceof HttpErrorResponse ? failure.status : null;
  });

  // Key points can be changed only while the tour is a draft; any other status is read-only.
  protected readonly isDraft = computed(() => this.tour.value()?.status === 'Draft');

  // The author's local changes, sent to the server only on Save. Null while there are none,
  // so the page shows the saved key points.
  private readonly editedKeyPoints = signal<KeyPointInputDto[] | null>(null);

  protected readonly keyPoints = computed<KeyPointInputDto[]>(
    () =>
      this.editedKeyPoints() ??
      (this.tour.value()?.keyPoints ?? []).map((point) => ({
        name: point.name,
        description: point.description,
        imageAddress: point.imageAddress,
        location: point.location,
        secret: point.secret,
        visibleWithoutPurchase: point.visibleWithoutPurchase,
      })),
  );

  // Every key point in the list has a location, because Confirm requires one.
  protected readonly locations = computed<MapLocation[]>(() =>
    this.keyPoints().flatMap((point) => (point.location === null ? [] : [point.location])),
  );

  // The length the overview map reports for the current list; null until it arrives.
  protected readonly routeLength = signal<number | null>(null);
  protected readonly straightLine = signal(false);

  protected readonly lengthText = computed(() => {
    const kilometres = this.routeLength();
    if (kilometres === null) {
      return null;
    }
    return this.straightLine()
      ? `about ${kilometres.toFixed(2)} km (straight line)`
      : `${kilometres.toFixed(2)} km`;
  });

  protected readonly pointForm = form(signal(EMPTY_POINT), (path) => {
    required(path.name, { message: 'Name is required.' });
    required(path.description, { message: 'Description is required.' });
    required(path.imageAddress, { message: 'Image is required.' });
    required(path.location, { message: 'Location is required. Click the map to place it.' });
  });

  protected readonly pointLocations = computed<MapLocation[]>(() => {
    const location = this.pointForm.location().value();
    return location === null ? [] : [location];
  });

  protected readonly formOpen = signal(false);
  // The key point the form changes, or null while the form adds a new one.
  protected readonly editingIndex = signal<number | null>(null);

  protected readonly selectedImage = signal<File | null>(null);
  protected readonly uploading = signal(false);
  protected readonly imageError = signal<string | null>(null);

  protected readonly pending = signal(false);
  protected readonly saveError = signal<string | null>(null);
  protected readonly success = signal<string | null>(null);

  // While the form is open or the list is being saved, the list itself does not change.
  protected readonly listLocked = computed(() => this.formOpen() || this.pending());

  protected openAdd(): void {
    this.clearMessages();
    this.clearImage();
    this.editingIndex.set(null);
    this.pointForm().reset(EMPTY_POINT);
    this.formOpen.set(true);
  }

  protected openEdit(index: number): void {
    const point = this.keyPoints()[index];
    this.clearMessages();
    this.clearImage();
    this.editingIndex.set(index);
    this.pointForm().reset({
      name: point.name,
      description: point.description,
      secret: point.secret ?? '',
      visibleWithoutPurchase: point.visibleWithoutPurchase,
      imageAddress: point.imageAddress,
      location: point.location,
    });
    this.formOpen.set(true);
  }

  protected cancel(): void {
    this.formOpen.set(false);
  }

  // A chosen file is not the key point's image yet; it only turns on Upload.
  protected selectImage(file: File | null): void {
    this.imageError.set(null);
    this.selectedImage.set(file);
  }

  // The key point keeps the address of its last successful upload, so a failed upload
  // leaves an existing image in place.
  protected async sendImage(): Promise<void> {
    const file = this.selectedImage();
    if (file === null) {
      return;
    }
    this.uploading.set(true);
    this.imageError.set(null);
    try {
      const saved = await this.imageUploading.upload(file);
      this.pointForm.imageAddress().value.set(saved.address);
      this.selectedImage.set(null);
    } catch (failure) {
      this.imageError.set(serverMessage(failure, 'Could not upload the image.'));
    } finally {
      this.uploading.set(false);
      this.pointForm.imageAddress().markAsTouched();
    }
  }

  protected chooseLocation(locations: MapLocation[]): void {
    this.pointForm.location().value.set(locations[0] ?? null);
    this.pointForm.location().markAsTouched();
  }

  protected confirm(event: Event): void {
    event.preventDefault();
    if (!this.pointForm().valid() || this.uploading()) {
      return;
    }
    const point: KeyPointInputDto = {
      name: this.pointForm.name().value(),
      description: this.pointForm.description().value(),
      imageAddress: this.pointForm.imageAddress().value(),
      location: this.pointForm.location().value(),
      secret: this.pointForm.secret().value() === '' ? null : this.pointForm.secret().value(),
      visibleWithoutPurchase: this.pointForm.visibleWithoutPurchase().value(),
    };
    const index = this.editingIndex();
    if (index === null) {
      this.changeList([...this.keyPoints(), point]);
    } else {
      this.changeList(this.keyPoints().map((current, i) => (i === index ? point : current)));
    }
    this.formOpen.set(false);
  }

  protected remove(index: number): void {
    this.clearMessages();
    this.changeList(this.keyPoints().filter((_, i) => i !== index));
  }

  // Swaps the key point with its neighbour above (-1) or below (+1).
  protected move(index: number, step: -1 | 1): void {
    const list = [...this.keyPoints()];
    const point = list[index];
    list[index] = list[index + step];
    list[index + step] = point;
    this.clearMessages();
    this.changeList(list);
  }

  protected async save(): Promise<void> {
    const kilometres = this.routeLength();
    if (kilometres === null) {
      return;
    }
    this.pending.set(true);
    this.clearMessages();
    try {
      await this.tourAuthoring.defineKeyPoints(this.id(), {
        keyPoints: this.keyPoints(),
        length: { kilometres, isStraightLine: this.straightLine() },
      });
      this.editedKeyPoints.set(null);
      this.clearLength();
      this.tour.reload();
      this.success.set('Key points saved.');
    } catch (failure) {
      this.saveError.set(serverMessage(failure, 'Could not save the key points.'));
    } finally {
      this.pending.set(false);
    }
  }

  // A changed list gets a new route, so the old length and its straight-line mark no longer
  // apply until the overview map reports the new ones.
  private changeList(list: KeyPointInputDto[]): void {
    this.editedKeyPoints.set(list);
    this.clearLength();
  }

  private clearLength(): void {
    this.routeLength.set(null);
    this.straightLine.set(false);
  }

  // A newly opened form starts without a chosen file or an upload error.
  private clearImage(): void {
    this.selectedImage.set(null);
    this.imageError.set(null);
  }

  // The next change, or opening the form, clears the messages of the previous save.
  private clearMessages(): void {
    this.saveError.set(null);
    this.success.set(null);
  }
}

// A new key point is not visible without purchase until the author marks it.
const EMPTY_POINT = {
  name: '',
  description: '',
  secret: '',
  visibleWithoutPurchase: false,
  imageAddress: '',
  location: null as MapLocation | null,
};
