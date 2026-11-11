import { Component, ElementRef, effect, inject, input, output } from '@angular/core';
import * as L from 'leaflet';

export interface MapLocation {
  latitude: number;
  longitude: number;
}

const TILES_URL = 'https://tile.openstreetmap.org/{z}/{x}/{y}.png';
const TILES_ATTRIBUTION =
  '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>';

// Bez ijedne lokacije mapa prikazuje Novi Sad.
const DEFAULT_CENTER = L.latLng(45.2671, 19.8335);
const DEFAULT_ZOOM = 12;
const MAX_FIT_ZOOM = 15;

@Component({
  selector: 'app-location-map',
  styleUrl: './location-map.scss',
  templateUrl: './location-map.html',
})
export class LocationMap {
  private readonly host: HTMLElement = inject(ElementRef).nativeElement;

  readonly locations = input<MapLocation[]>([]);
  readonly showRoute = input(false);

  readonly routeLength = output<number>();

  constructor() {
    // Leaflet crta van stabla komponenti, pa effect() ponovo iscrtava mapu kada se ulazi promene.
    effect((onCleanup) => {
      const map = this.draw(this.locations(), this.showRoute());
      onCleanup(() => map.remove());
    });
  }

  private draw(locations: MapLocation[], showRoute: boolean): L.Map {
    const map = L.map(this.host);
    L.tileLayer(TILES_URL, { attribution: TILES_ATTRIBUTION }).addTo(map);

    const points = locations.map((location) => L.latLng(location.latitude, location.longitude));
    if (points.length === 0) {
      map.setView(DEFAULT_CENTER, DEFAULT_ZOOM);
    } else {
      map.fitBounds(L.latLngBounds(points), { maxZoom: MAX_FIT_ZOOM, padding: [24, 24] });
    }

    const color = getComputedStyle(this.host).getPropertyValue('--color-accent');
    for (const point of points) {
      L.circleMarker(point, { className: 'map-pin', color, radius: 8 }).addTo(map);
    }

    if (showRoute) {
      if (points.length >= 2) {
        L.polyline(points, { className: 'map-route', color }).addTo(map);
      }
      this.routeLength.emit(lengthInKilometres(points));
    }
    return map;
  }
}

function lengthInKilometres(points: L.LatLng[]): number {
  let metres = 0;
  for (let i = 1; i < points.length; i++) {
    metres += points[i - 1].distanceTo(points[i]);
  }
  return metres / 1000;
}
