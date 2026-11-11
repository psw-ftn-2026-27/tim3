import { Component, signal } from '@angular/core';
import { LocationMap, MapLocation } from '../../shared/map/location-map/location-map';

@Component({
  imports: [LocationMap],
  selector: 'app-map',
  styleUrl: './map.scss',
  templateUrl: './map.html',
})
export class MapPage {
  // Nekoliko poznatih mesta u Novom Sadu, samo za prikaz komponente.
  protected readonly locations: MapLocation[] = [
    { latitude: 45.2551, longitude: 19.8451 },
    { latitude: 45.2556, longitude: 19.8521 },
    { latitude: 45.2522, longitude: 19.8622 },
    { latitude: 45.2461, longitude: 19.8517 },
  ];

  protected readonly showRoute = signal(false);
  protected readonly routeLength = signal(0);

  protected toggleRoute(): void {
    this.showRoute.update((shown) => !shown);
  }
}
