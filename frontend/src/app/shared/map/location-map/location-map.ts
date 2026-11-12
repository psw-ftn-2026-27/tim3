import { httpResource } from '@angular/common/http';
import {
  Component,
  ElementRef,
  computed,
  effect,
  inject,
  input,
  output,
  signal,
} from '@angular/core';
import * as L from 'leaflet';
import { WalkingRouteDto } from '../../api/maps';

export interface MapLocation {
  latitude: number;
  longitude: number;
}

// Vrsta prikaza putanje: bez putanje, pravim linijama ili po ulicama.
export type RouteKind = 'none' | 'straight' | 'streets';

const TILES_URL = 'https://tile.openstreetmap.org/{z}/{x}/{y}.png';
const TILES_ATTRIBUTION =
  '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>';

// Bez ijedne lokacije mapa prikazuje Novi Sad.
const DEFAULT_CENTER = L.latLng(45.2671, 19.8335);
const DEFAULT_ZOOM = 12;
const MAX_FIT_ZOOM = 15;

// Koliko dugo se vidi poruka da je unet najveći dozvoljen broj pinova.
const LIMIT_MESSAGE_MS = 3000;

@Component({
  selector: 'app-location-map',
  styleUrl: './location-map.scss',
  templateUrl: './location-map.html',
})
export class LocationMap {
  private readonly host: HTMLElement = inject(ElementRef).nativeElement;

  readonly locations = input<MapLocation[]>([]);
  readonly route = input<RouteKind>('none');
  // Izbor niza: klik na mapu dodaje pin, prevlačenje ga pomera, klik na pin ga uklanja.
  readonly selectable = input(false);
  // Najveći broj pinova u izboru; null znači bez granice.
  readonly maxPins = input<number | null>(null);

  readonly routeLength = output<number>();
  readonly routeNotAlongStreets = output<void>();
  // Ceo nov niz posle svakog dodavanja, uklanjanja i puštanja prevučenog pina. Komponenta
  // niz ne čuva: stranica ga upisuje u svoj signal i vraća kroz ulaz locations.
  readonly locationsChange = output<MapLocation[]>();

  protected readonly limitReached = signal(false);
  private limitMessageTimer: ReturnType<typeof setTimeout> | undefined;

  // Odobren izuzetak: putanju po ulicama čita sama komponenta, samo dok je izabrana
  // i ima bar dve lokacije. Bez adrese (undefined) zahtev se ne šalje.
  private readonly walkingRoute = httpResource<WalkingRouteDto>(() => {
    const locations = this.locations();
    if (this.route() !== 'streets' || locations.length < 2) {
      return undefined;
    }
    return walkingRouteUrl(locations);
  });

  // Svaka greška upita (nema puta, servis ne odgovara, korisnik nije prijavljen)
  // znači da putanja nije po ulicama, pa se crtaju prave linije.
  protected readonly notAlongStreets = computed(
    () =>
      this.route() === 'streets' &&
      this.locations().length >= 2 &&
      this.walkingRoute.status() === 'error',
  );

  constructor() {
    // Leaflet crta van stabla komponenti, pa effect() ponovo iscrtava mapu kada se ulazi
    // ili odgovor servera promene.
    effect((onCleanup) => {
      const map = this.draw(this.locations(), this.route(), this.selectable());
      onCleanup(() => map.remove());
    });
  }

  private draw(locations: MapLocation[], route: RouteKind, selectable: boolean): L.Map {
    const map = L.map(this.host);
    L.tileLayer(TILES_URL, { attribution: TILES_ATTRIBUTION }).addTo(map);

    const points = locations.map(toLatLng);
    if (points.length === 0) {
      map.setView(DEFAULT_CENTER, DEFAULT_ZOOM);
    } else {
      map.fitBounds(L.latLngBounds(points), { maxZoom: MAX_FIT_ZOOM, padding: [24, 24] });
    }

    const color = getComputedStyle(this.host).getPropertyValue('--color-accent');
    if (selectable) {
      map.on('click', (event) => this.addLocation(event.latlng));
      points.forEach((point, i) => this.addNumberedPin(map, point, i));
    } else {
      for (const point of points) {
        L.circleMarker(point, { className: 'map-pin', color, radius: 8 }).addTo(map);
      }
    }

    if (route === 'none') {
      return map;
    }
    if (points.length < 2) {
      this.routeLength.emit(0);
      return map;
    }

    if (route === 'straight' || this.notAlongStreets()) {
      L.polyline(points, { className: 'map-route', color }).addTo(map);
      this.routeLength.emit(lengthInKilometres(points));
      if (route === 'streets') {
        this.routeNotAlongStreets.emit();
      }
      return map;
    }

    // Po ulicama: dok odgovor ne stigne, vide se samo pinovi.
    if (this.walkingRoute.hasValue()) {
      const walkingRoute = this.walkingRoute.value();
      L.polyline(walkingRoute.points.map(toLatLng), { className: 'map-route', color }).addTo(map);
      this.routeLength.emit(walkingRoute.lengthInKilometres);
    }
    return map;
  }

  // Pin sa rednim brojem. Njegov HTML pravi Leaflet van šablona, pa se stilizuje inline iz tokena.
  private addNumberedPin(map: L.Map, point: L.LatLng, index: number): void {
    const icon = L.divIcon({
      className: 'map-pin',
      html:
        '<span style="display: flex; align-items: center; justify-content: center; ' +
        'width: 100%; height: 100%; border-radius: 50%; background: var(--color-accent); ' +
        'color: var(--color-accent-text); border: 2px solid var(--color-surface); ' +
        `font-size: var(--font-size-sm); font-weight: bold;">${index + 1}</span>`,
      iconSize: [28, 28],
    });
    const marker = L.marker(point, { draggable: true, icon }).addTo(map);
    // Tokom prevlačenja Leaflet pomera samo pin; niz se javlja tek kada korisnik pusti pin.
    marker.on('dragend', () => this.moveLocation(index, marker.getLatLng()));
    marker.on('click', () => this.removeLocation(index));
  }

  private addLocation(point: L.LatLng): void {
    const locations = this.locations();
    const maxPins = this.maxPins();
    const location = toMapLocation(point);

    if (maxPins === 1 && locations.length >= 1) {
      this.locationsChange.emit([location]);
      return;
    }
    if (maxPins !== null && locations.length >= maxPins) {
      this.showLimitMessage();
      return;
    }
    this.locationsChange.emit([...locations, location]);
  }

  private moveLocation(index: number, point: L.LatLng): void {
    this.locationsChange.emit(
      this.locations().map((location, i) => (i === index ? toMapLocation(point) : location)),
    );
  }

  private removeLocation(index: number): void {
    this.locationsChange.emit(this.locations().filter((_, i) => i !== index));
  }

  private showLimitMessage(): void {
    this.limitReached.set(true);
    clearTimeout(this.limitMessageTimer);
    this.limitMessageTimer = setTimeout(() => this.limitReached.set(false), LIMIT_MESSAGE_MS);
  }
}

function toMapLocation(point: L.LatLng): MapLocation {
  return { latitude: point.lat, longitude: point.lng };
}

function toLatLng(location: MapLocation): L.LatLng {
  return L.latLng(location.latitude, location.longitude);
}

function walkingRouteUrl(locations: MapLocation[]): string {
  const query = locations
    .map(
      (location, i) =>
        `locations[${i}].latitude=${location.latitude}&locations[${i}].longitude=${location.longitude}`,
    )
    .join('&');
  return `/api/maps/walking-routes?${query}`;
}

function lengthInKilometres(points: L.LatLng[]): number {
  let metres = 0;
  for (let i = 1; i < points.length; i++) {
    metres += points[i - 1].distanceTo(points[i]);
  }
  return metres / 1000;
}
