import { Component, signal } from '@angular/core';
import { LocationMap, MapLocation, RouteKind } from '../../shared/map/location-map/location-map';

// Nekoliko poznatih mesta u Novom Sadu, samo za prikaz komponente.
const START_LOCATIONS: MapLocation[] = [
  { latitude: 45.2551, longitude: 19.8451 },
  { latitude: 45.2556, longitude: 19.8521 },
  { latitude: 45.2522, longitude: 19.8622 },
  { latitude: 45.2461, longitude: 19.8517 },
];

@Component({
  imports: [LocationMap],
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
}
