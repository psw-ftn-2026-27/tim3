import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { SetUserPositionDto } from '../../../shared/api/maps';

const BASE_URL = '/api/games/location-challenges/solving';

// The explorer's position belongs to the Maps module. The pages of this group are pages where
// the explorer sets it by clicking the map, so they save it through the Maps endpoint
// (shared/map/README.md, "Postavljanje pozicije").
const POSITION_URL = '/api/maps/user-positions/me';

@Injectable({ providedIn: 'root' })
export class LocationChallengeSolving {
  private readonly http = inject(HttpClient);

  // The server answers with the id of the new participation.
  async activate(id: string): Promise<string> {
    return firstValueFrom(this.http.post<string>(`${BASE_URL}/${id}/activation`, null));
  }

  async checkLocation(id: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/location-check`, null));
  }

  async setPosition(position: SetUserPositionDto): Promise<void> {
    await firstValueFrom(this.http.put<void>(POSITION_URL, position));
  }
}
