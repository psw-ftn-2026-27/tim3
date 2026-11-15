import { HttpErrorResponse, httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { FormField, form, validate } from '@angular/forms/signals';
import { UserPositionDto } from '../../shared/api/maps';
import {
  LocationMap,
  MapCircle,
  MapLocation,
  RouteKind,
} from '../../shared/map/location-map/location-map';
import { UserPositioning } from './user-positioning';

// Nekoliko poznatih mesta u Novom Sadu, samo za prikaz komponente.
const START_LOCATIONS: MapLocation[] = [
  { latitude: 45.2551, longitude: 19.8451 },
  { latitude: 45.2556, longitude: 19.8521 },
  { latitude: 45.2522, longitude: 19.8622 },
  { latitude: 45.2461, longitude: 19.8517 },
];

@Component({
  imports: [FormField, LocationMap],
  selector: 'app-map',
  styleUrl: './map.scss',
  templateUrl: './map.html',
})
export class MapPage {
  // Niz drži stranica: mapa javlja nov niz, a stranica ga vraća mapi kroz ulaz locations.
  protected readonly locations = signal<MapLocation[]>(START_LOCATIONS);
  protected readonly maxPins = signal<number | null>(null);

  protected readonly route = signal<RouteKind>('none');
  // null dok mapa ne javi dužinu za izabranu vrstu prikaza (na primer dok čeka putanju po ulicama).
  protected readonly routeLength = signal<number | null>(null);

  private readonly userPositioning = inject(UserPositioning);

  // Režim klika: uvek je uključen tačno jedan, pa se izbor kruga i izbor niza ne uključuju zajedno.
  protected readonly clickMode = signal<'sequence' | 'position' | 'circle'>('sequence');
  protected readonly settingPosition = computed(() => this.clickMode() === 'position');
  protected readonly pickingCircle = computed(() => this.clickMode() === 'circle');

  // Centar kruga drži stranica: mapa javlja mesto klika, a stranica ga vraća mapi kroz ulaz circle.
  protected readonly circleCenter = signal<MapLocation | null>(null);
  protected readonly radiusForm = form(
    signal<{ radius: number | null }>({ radius: 500 }),
    (path) => {
      validate(path.radius, ({ value }) => {
        const radius = value();
        return radius !== null && radius > 0
          ? null
          : { kind: 'positive', message: 'Poluprečnik mora biti veći od 0.' };
      });
    },
  );
  // Bez centra ili dok poluprečnik nije ispravan, stranica mapi ne predaje krug.
  protected readonly circle = computed<MapCircle | null>(() => {
    const center = this.circleCenter();
    const radius = this.radiusForm.radius().value();
    if (center === null || radius === null || !this.radiusForm.radius().valid()) {
      return null;
    }
    return { center, radiusInMetres: radius };
  });
  // Sačuvana pozicija korisnika. Dok se učitava ili kada čitanje ne uspe, resurs nema
  // vrednost i mapa ne crta pin.
  protected readonly userPosition = httpResource<UserPositionDto>(
    () => '/api/maps/user-positions/me',
  );
  protected readonly position = computed<MapLocation | null>(() => {
    if (!this.userPosition.hasValue()) {
      return null;
    }
    const position = this.userPosition.value();
    return { latitude: position.latitude, longitude: position.longitude };
  });
  // Samo 404 znači da korisnik još nije postavio poziciju; 401 ili 500 to ne znače.
  protected readonly positionErrorStatus = computed(() => {
    const failure = this.userPosition.error();
    return failure instanceof HttpErrorResponse ? failure.status : null;
  });
  protected readonly positionMissing = computed(() => this.positionErrorStatus() === 404);
  protected readonly positionError = signal<string | null>(null);

  protected select(route: RouteKind): void {
    if (route === this.route()) {
      return;
    }
    this.route.set(route);
    this.routeLength.set(null);
  }

  // Promena granice vraća niz na početni: četiri lokacije, a za granicu 1 praznu mapu.
  protected selectLimit(maxPins: number | null): void {
    if (maxPins === this.maxPins()) {
      return;
    }
    this.maxPins.set(maxPins);
    this.locations.set(maxPins === 1 ? [] : START_LOCATIONS);
    this.routeLength.set(null);
  }

  protected changeLocations(locations: MapLocation[]): void {
    this.locations.set(locations);
    this.routeLength.set(null);
  }

  // Pin se premešta tek kada server sačuva poziciju i stranica je ponovo pročita; dok se
  // to ne desi, pin ostaje na prethodnom mestu.
  protected async setPosition(position: MapLocation): Promise<void> {
    this.positionError.set(null);
    try {
      await this.userPositioning.setPosition(position);
      this.userPosition.reload();
    } catch {
      this.positionError.set('Pozicija nije sačuvana.');
    }
  }
}
