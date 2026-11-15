import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { SetUserPositionDto } from '../../shared/api/maps';

const BASE_URL = '/api/maps/user-positions';

@Injectable({ providedIn: 'root' })
export class UserPositioning {
  private readonly http = inject(HttpClient);

  async setPosition(position: SetUserPositionDto): Promise<void> {
    await firstValueFrom(this.http.put<void>(`${BASE_URL}/me`, position));
  }
}
