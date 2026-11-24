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

// Krug oko centra, poluprečnika u metrima mereno po površini Zemlje.
export interface MapCircle {
  center: MapLocation;
  radiusInMetres: number;
}

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
  // Postavljanje pozicije nudi samo stranica koja to izabere; podrazumevano ga mapa ne nudi.
  readonly positionSettable = input(false);
  // Režim klika: dok je uključen (i postavljanje ponuđeno), klik javlja poziciju umesto da menja niz.
  readonly settingPosition = input(false);
  // Sačuvana pozicija korisnika; null dok nije poznata ili ne postoji.
  readonly position = input<MapLocation | null>(null);
  // Stranica javlja da server zna da korisnik još nije postavio poziciju. Dok se pozicija
  // učitava ili čitanje ne uspe, ovo ostaje false, pa mapa ne tvrdi da pozicije nema.
  readonly positionMissing = input(false);
  // Krug koji mapa crta; null znači bez kruga. Poluprečnik zadaje stranica.
  readonly circle = input<MapCircle | null>(null);
  // Režim klika: dok je uključen, klik na mapu javlja centar kruga umesto da menja niz.
  // Postavljanje pozicije ima prednost, pa dok je ono uključeno klik ne javlja centar.
  readonly pickingCircle = input(false);
  // Izbor pina: klik na pin ga ističe i javlja njegov indeks. Izbor niza ima prednost, pa uz
  // selectable izbor pina nije uključen; isto važi dok je uključeno postavljanje pozicije.
  readonly pickingPin = input(false);

  readonly routeLength = output<number>();
  readonly routeNotAlongStreets = output<void>();
  // Ceo nov niz posle svakog dodavanja, uklanjanja i puštanja prevučenog pina. Komponenta
  // niz ne čuva: stranica ga upisuje u svoj signal i vraća kroz ulaz locations.
  readonly locationsChange = output<MapLocation[]>();
  // Mesto klika u režimu postavljanja pozicije. Komponenta poziciju ne čuva: stranica je šalje
  // serveru, ponovo je pročita i vraća kroz ulaz position.
  readonly positionSet = output<MapLocation>();
  // Mesto klika u izboru kruga. Komponenta centar ne čuva: stranica ga upisuje u svoj signal
  // i vraća kroz ulaz circle.
  readonly circleCenterSet = output<MapLocation>();
  // Indeks izabrane lokacije u nizu locations koji je stranica predala, a ne njene koordinate.
  // Stranica preko indeksa nalazi svoj entitet.
  readonly pinPicked = output<number>();

  protected readonly showPositionMissing = computed(
    () => this.positionSettable() && this.positionMissing(),
  );

  // Istaknut pin u izboru pina; null dok nijedan nije izabran ili kada stignu nove lokacije.
  // Obično polje, ne signal: čita ga samo crtanje, pa klik na pin ne sme da ponovo iscrta mapu.
  private pickedPin: number | null = null;

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

  // Pogled sa prethodno iscrtane mape i šta je na njoj bilo, da bi pomeranje samog pina
  // pozicije zadržalo pomeranje i zum koje je korisnik namestio.
  private previousView: {
    locations: MapLocation[];
    position: MapLocation | null;
    circle: MapCircle | null;
    center: L.LatLng;
    zoom: number;
  } | null = null;

  constructor() {
    // Leaflet crta van stabla komponenti, pa effect() ponovo iscrtava mapu kada se ulazi
    // ili odgovor servera promene.
    effect((onCleanup) => {
      const locations = this.locations();
      const position = this.position();
      const circle = this.circle();
      const map = this.draw(locations, this.route(), this.selectable(), position, circle);
      onCleanup(() => {
        this.previousView = {
          locations,
          position,
          circle,
          center: map.getCenter(),
          zoom: map.getZoom(),
        };
        map.remove();
      });
    });
  }

  private draw(
    locations: MapLocation[],
    route: RouteKind,
    selectable: boolean,
    position: MapLocation | null,
    circle: MapCircle | null,
  ): L.Map {
    // Nove lokacije brišu istaknut pin, a ponovno crtanje istih lokacija ga zadržava.
    if (this.previousView === null || this.previousView.locations !== locations) {
      this.pickedPin = null;
    }

    const map = L.map(this.host);
    L.tileLayer(TILES_URL, { attribution: TILES_ATTRIBUTION }).addTo(map);

    const points = locations.map(toLatLng);
    // Mapa obuhvata sve pinove, i pin pozicije, i ceo krug.
    const framed = position === null ? points : [...points, toLatLng(position)];
    const circleBounds =
      circle === null ? null : toLatLng(circle.center).toBounds(2 * circle.radiusInMetres);
    // Kada se promeni samo pin pozicije koji je već bio na mapi, korisnik ga je postavio
    // klikom na deo mape koji vidi, pa mapa zadržava njegov pogled.
    const previous = this.previousView;
    if (
      previous !== null &&
      previous.locations === locations &&
      previous.circle === circle &&
      previous.position !== null &&
      position !== null &&
      previous.position !== position
    ) {
      map.setView(previous.center, previous.zoom);
    } else if (framed.length === 0 && circleBounds === null) {
      map.setView(DEFAULT_CENTER, DEFAULT_ZOOM);
    } else {
      const bounds = L.latLngBounds(framed);
      if (circleBounds !== null) {
        bounds.extend(circleBounds);
      }
      map.fitBounds(bounds, { maxZoom: MAX_FIT_ZOOM, padding: [24, 24] });
    }

    // Dok se postavlja pozicija, klik javlja poziciju, a pinovi niza se samo prikazuju,
    // pa klik na njih ide mapi.
    const settingPosition = this.positionSettable() && this.settingPosition();
    // U izboru kruga klik javlja centar, a pinovi niza se isto samo prikazuju.
    const pickingCircle = !settingPosition && this.pickingCircle();
    if (settingPosition) {
      map.on('click', (event) => this.positionSet.emit(toMapLocation(event.latlng)));
    } else if (pickingCircle) {
      map.on('click', (event) => this.circleCenterSet.emit(toMapLocation(event.latlng)));
    }
    if (position !== null) {
      this.addPositionPin(map, toLatLng(position));
    }

    const color = getComputedStyle(this.host).getPropertyValue('--color-accent');
    if (circle !== null) {
      // Krug ne prima klik, pa klik unutar njega ide mapi.
      L.circle(toLatLng(circle.center), {
        className: 'map-circle',
        color,
        interactive: false,
        radius: circle.radiusInMetres,
      }).addTo(map);
    }
    const editable = !settingPosition && !pickingCircle;
    // Izbor pina radi i uz izbor kruga: klik na pin ne stiže do mape, pa ne javlja centar.
    const pickingPin = !selectable && !settingPosition && this.pickingPin();
    if (selectable) {
      if (editable) {
        map.on('click', (event) => this.addLocation(event.latlng));
      }
      points.forEach((point, i) => this.addNumberedPin(map, point, i, editable));
    } else if (pickingPin) {
      this.addPickablePins(map, points);
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
  private addNumberedPin(map: L.Map, point: L.LatLng, index: number, editable: boolean): void {
    const icon = L.divIcon({
      className: 'map-pin',
      html:
        '<span style="display: flex; align-items: center; justify-content: center; ' +
        'width: 100%; height: 100%; border-radius: 50%; background: var(--color-accent); ' +
        'color: var(--color-accent-text); border: 2px solid var(--color-surface); ' +
        `font-size: var(--font-size-sm); font-weight: bold;">${index + 1}</span>`,
      iconSize: [28, 28],
    });
    const marker = L.marker(point, { draggable: editable, icon, interactive: editable }).addTo(map);
    if (!editable) {
      return;
    }
    // Tokom prevlačenja Leaflet pomera samo pin; niz se javlja tek kada korisnik pusti pin.
    marker.on('dragend', () => this.moveLocation(index, marker.getLatLng()));
    marker.on('click', () => this.removeLocation(index));
  }

  // Pinovi u izboru pina su bez broja. Istaknut pin se menja na samoj mapi, bez ponovnog crtanja,
  // pa mapa zadržava pogled koji je korisnik namestio.
  private addPickablePins(map: L.Map, points: L.LatLng[]): void {
    const picked = this.pickedPin;
    const pins = points.map((point, i) =>
      L.marker(point, { icon: pickablePinIcon(i === picked) }).addTo(map),
    );
    pins.forEach((pin, index) =>
      pin.on('click', () => {
        this.pickedPin = index;
        pins.forEach((other, i) => other.setIcon(pickablePinIcon(i === index)));
        this.pinPicked.emit(index);
      }),
    );
  }

  // Pin pozicije je kvadrat bez broja, drugačiji od okruglih pinova niza. Ne prima klik, pa
  // klik na njega ide mapi.
  private addPositionPin(map: L.Map, point: L.LatLng): void {
    const icon = L.divIcon({
      className: 'map-position',
      html:
        '<span style="display: block; width: 100%; height: 100%; ' +
        'background: var(--color-success); border: 3px solid var(--color-surface); ' +
        'outline: 2px solid var(--color-success);"></span>',
      iconSize: [18, 18],
    });
    L.marker(point, { icon, interactive: false }).addTo(map);
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

// Pin bez broja. Njegov HTML pravi Leaflet van šablona, pa se stilizuje inline iz tokena;
// istaknut pin je veći, sa debljim okvirom u boji akcenta.
function pickablePinIcon(picked: boolean): L.DivIcon {
  const size = picked ? 28 : 18;
  const border = picked ? 6 : 3;
  return L.divIcon({
    className: picked ? 'map-pin map-pin--picked' : 'map-pin',
    html:
      '<span style="display: block; box-sizing: border-box; width: 100%; height: 100%; ' +
      'border-radius: 50%; background: var(--color-surface); ' +
      `border: ${border}px solid var(--color-accent);"></span>`,
    iconSize: [size, size],
  });
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
