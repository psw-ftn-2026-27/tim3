import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { ImageAddressDto } from '../api/image-address-dto';

const BASE_URL = '/api/media/images';

@Injectable({ providedIn: 'root' })
export class ImageUploading {
  private readonly http = inject(HttpClient);

  async upload(file: File): Promise<ImageAddressDto> {
    const body = new FormData();
    body.append('file', file);
    return firstValueFrom(this.http.post<ImageAddressDto>(BASE_URL, body));
  }
}
