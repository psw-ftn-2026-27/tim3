import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import * as L from 'leaflet';
import { LocationMap, MapLocation, RouteKind } from './location-map';

// Jedan stepen geografske dužine na ekvatoru, mereno po površini Zemlje (poluprečnik 6371 km).
const ONE_DEGREE_KM = (Math.PI * 6371) / 180;

const WALKING_ROUTES_URL = '/api/maps/walking-routes';
const MESSAGE = 'Putanja nije po ulicama: prikazane su prave linije.';
const LIMIT_MESSAGE = 'Uneli ste najveći dozvoljen broj pinova.';
const NO_POSITION_MESSAGE = 'Pozicija još nije postavljena.';

describe('LocationMap', () => {
  let fixture: ComponentFixture<LocationMap>;
  let httpTesting: HttpTestingController;
  let lengths: number[];
  let notAlongStreets: number;
  let addedLines: ReturnType<typeof vi.spyOn>;
  let addedTiles: ReturnType<typeof vi.spyOn>;
  let addedPins: ReturnType<typeof vi.spyOn>;
  let changes: MapLocation[][];
  let positions: MapLocation[];

  function show(locations: MapLocation[], route: RouteKind): void {
    fixture.componentRef.setInput('locations', locations);
    fixture.componentRef.setInput('route', route);
    fixture.detectChanges();
  }

  function count(selector: string): number {
    return fixture.nativeElement.querySelectorAll(selector).length;
  }

  // Tačke poslednje linije dodate na mapu. jsdom ne raspoređuje elemente, pa Leaflet seče SVG
  // putanju na prazan prikaz i tačke se ne mogu pročitati iz nje, nego iz same linije.
  function lastRoutePoints(): L.LatLng[] {
    const lines = addedLines.mock.contexts as L.Polyline[];
    return lines[lines.length - 1].getLatLngs() as L.LatLng[];
  }

  // Izbor niza: mapa i pinovi se čitaju iz Leaflet-a, a klik i prevlačenje se izazivaju
  // događajima, jer jsdom ne raspoređuje elemente pa se ne može kliknuti na tačno mesto.
  function select(locations: MapLocation[], route: RouteKind, maxPins: number | null = null): void {
    fixture.componentRef.setInput('selectable', true);
    fixture.componentRef.setInput('maxPins', maxPins);
    show(locations, route);
  }

  // Kao stranica: nov niz koji mapa javi vraća joj se kroz ulaz.
  function writeBack(): void {
    fixture.componentRef.setInput('locations', changes.at(-1));
    fixture.detectChanges();
  }

  function clickOnMap(location: MapLocation): void {
    const map = addedTiles.mock.calls.at(-1)![0] as L.Map;
    map.fire('click', { latlng: L.latLng(location.latitude, location.longitude) });
    fixture.detectChanges();
  }

  // Pinovi sa trenutno prikazane mape, redom kojim su dodati.
  function currentPins(): L.Marker[] {
    const map = addedTiles.mock.calls.at(-1)![0] as L.Map;
    return (addedPins.mock.contexts as L.Marker[]).filter((pin) => map.hasLayer(pin));
  }

  function pinNumbers(): string[] {
    return Array.from(
      fixture.nativeElement.querySelectorAll('.map-pin') as NodeListOf<HTMLElement>,
    ).map((pin) => pin.textContent!.trim());
  }

  // Mapa koja nudi postavljanje pozicije, sa izborom niza i izabranim režimom klika.
  function offerPosition(
    locations: MapLocation[],
    settingPosition: boolean,
    position: MapLocation | null = null,
    positionMissing = false,
  ): void {
    fixture.componentRef.setInput('positionSettable', true);
    fixture.componentRef.setInput('settingPosition', settingPosition);
    fixture.componentRef.setInput('position', position);
    fixture.componentRef.setInput('positionMissing', positionMissing);
    select(locations, 'none');
  }

  function walkingRouteRequests(): number {
    return httpTesting.match((request) => request.url.startsWith(WALKING_ROUTES_URL)).length;
  }

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    httpTesting = TestBed.inject(HttpTestingController);
    addedLines = vi.spyOn(L.Polyline.prototype, 'onAdd');
    addedTiles = vi.spyOn(L.TileLayer.prototype, 'onAdd');
    addedPins = vi.spyOn(L.Marker.prototype, 'onAdd');
    fixture = TestBed.createComponent(LocationMap);
    lengths = [];
    notAlongStreets = 0;
    changes = [];
    positions = [];
    fixture.componentInstance.positionSet.subscribe((position) => positions.push(position));
    fixture.componentInstance.locationsChange.subscribe((locations) => changes.push(locations));
    fixture.componentInstance.routeLength.subscribe((length) => lengths.push(length));
    fixture.componentInstance.routeNotAlongStreets.subscribe(() => notAlongStreets++);
  });

  afterEach(() => {
    addedLines.mockRestore();
    addedTiles.mockRestore();
    addedPins.mockRestore();
    httpTesting.verify();
  });

  it('sabira rastojanja susednih lokacija redom kojim su predate', () => {
    show(
      [
        { latitude: 0, longitude: 0 },
        { latitude: 0, longitude: 1 },
        { latitude: 0, longitude: 3 },
      ],
      'straight',
    );
    expect(lengths.at(-1)).toBeCloseTo(3 * ONE_DEGREE_KM, 6);

    show(
      [
        { latitude: 0, longitude: 0 },
        { latitude: 0, longitude: 3 },
        { latitude: 0, longitude: 1 },
      ],
      'straight',
    );
    expect(lengths.at(-1)).toBeCloseTo(5 * ONE_DEGREE_KM, 6);
  });

  it('sa jednom lokacijom ili bez lokacija daje 0 km i ne crta liniju', () => {
    show([{ latitude: 45.25, longitude: 19.85 }], 'straight');
    expect(lengths.at(-1)).toBe(0);
    expect(count('.map-route')).toBe(0);

    show([], 'straight');
    expect(lengths.at(-1)).toBe(0);
    expect(count('.map-route')).toBe(0);
  });

  it('prikazuje po jedan pin za svaku lokaciju, a putanju samo kada je zatražena', () => {
    const locations = [
      { latitude: 45.25, longitude: 19.84 },
      { latitude: 45.26, longitude: 19.85 },
      { latitude: 45.24, longitude: 19.86 },
    ];

    show(locations, 'none');
    expect(count('.map-pin')).toBe(3);
    expect(count('.map-route')).toBe(0);
    expect(lengths).toEqual([]);

    show(locations, 'straight');
    expect(count('.map-pin')).toBe(3);
    expect(count('.map-route')).toBe(1);
  });

  it('kada dobije nove lokacije, uklanja stare pinove i ponovo računa dužinu', () => {
    show(
      [
        { latitude: 0, longitude: 0 },
        { latitude: 0, longitude: 1 },
        { latitude: 0, longitude: 2 },
      ],
      'straight',
    );
    expect(count('.map-pin')).toBe(3);
    expect(lengths.at(-1)).toBeCloseTo(2 * ONE_DEGREE_KM, 6);

    show(
      [
        { latitude: 0, longitude: 0 },
        { latitude: 0, longitude: 4 },
      ],
      'straight',
    );
    expect(count('.map-pin')).toBe(2);
    expect(lengths.at(-1)).toBeCloseTo(4 * ONE_DEGREE_KM, 6);
  });

  it('bez izabrane vrste prikaza ne crta putanju, ne javlja dužinu i ne šalje zahtev', () => {
    fixture.componentRef.setInput('locations', [
      { latitude: 0, longitude: 0 },
      { latitude: 0, longitude: 1 },
    ]);
    fixture.detectChanges();
    TestBed.tick();

    expect(count('.map-pin')).toBe(2);
    expect(count('.map-route')).toBe(0);
    expect(lengths).toEqual([]);
    expect(walkingRouteRequests()).toBe(0);
  });

  it('traži putanju od servera samo kada je izabran prikaz po ulicama', () => {
    const locations = [
      { latitude: 45.25, longitude: 19.84 },
      { latitude: 45.26, longitude: 19.85 },
    ];

    show(locations, 'none');
    TestBed.tick();
    expect(walkingRouteRequests()).toBe(0);

    show(locations, 'straight');
    TestBed.tick();
    expect(walkingRouteRequests()).toBe(0);

    show(locations, 'streets');
    TestBed.tick();
    httpTesting.expectOne(
      `${WALKING_ROUTES_URL}?locations[0].latitude=45.25&locations[0].longitude=19.84` +
        '&locations[1].latitude=45.26&locations[1].longitude=19.85',
    );
  });

  it('kada putanja po ulicama stigne, crta liniju iz vraćenih tačaka i javlja dužinu sa servera', async () => {
    show(
      [
        { latitude: 0, longitude: 0 },
        { latitude: 0, longitude: 1 },
      ],
      'streets',
    );
    TestBed.tick();
    // Dok se čeka odgovor, vide se samo pinovi.
    expect(count('.map-pin')).toBe(2);
    expect(count('.map-route')).toBe(0);
    expect(lengths).toEqual([]);

    httpTesting
      .expectOne((request) => request.url.startsWith(WALKING_ROUTES_URL))
      .flush({
        points: [
          { latitude: 0, longitude: 0 },
          { latitude: 0.5, longitude: 0.5 },
          { latitude: 0, longitude: 1 },
        ],
        lengthInKilometres: 150,
      });
    await fixture.whenStable();

    expect(count('.map-route')).toBe(1);
    expect(lastRoutePoints().map((point) => [point.lat, point.lng])).toEqual([
      [0, 0],
      [0.5, 0.5],
      [0, 1],
    ]);
    expect(lengths.at(-1)).toBe(150);
    expect(notAlongStreets).toBe(0);
    expect(fixture.nativeElement.textContent).not.toContain(MESSAGE);
  });

  it('kada upit za putanju završi greškom, crta prave linije, prikazuje poruku i javlja vazdušnu dužinu', async () => {
    show(
      [
        { latitude: 0, longitude: 0 },
        { latitude: 0, longitude: 1 },
        { latitude: 0, longitude: 3 },
      ],
      'streets',
    );
    TestBed.tick();

    httpTesting
      .expectOne((request) => request.url.startsWith(WALKING_ROUTES_URL))
      .flush(null, { status: 404, statusText: 'Not Found' });
    await fixture.whenStable();

    expect(count('.map-route')).toBe(1);
    expect(lengths.at(-1)).toBeCloseTo(3 * ONE_DEGREE_KM, 6);
    expect(notAlongStreets).toBe(1);
    expect(fixture.nativeElement.textContent).toContain(MESSAGE);
  });

  it('sa manje od dve lokacije u prikazu po ulicama ne šalje zahtev, ne crta liniju i daje 0 km', () => {
    show([{ latitude: 45.25, longitude: 19.85 }], 'streets');
    TestBed.tick();
    expect(walkingRouteRequests()).toBe(0);
    expect(count('.map-route')).toBe(0);
    expect(lengths.at(-1)).toBe(0);

    show([], 'streets');
    TestBed.tick();
    expect(walkingRouteRequests()).toBe(0);
    expect(count('.map-route')).toBe(0);
    expect(lengths.at(-1)).toBe(0);
  });

  it('u izboru klik na praznu mapu dodaje lokaciju na kraj niza i putanja ide kroz ceo nov niz', () => {
    select(
      [
        { latitude: 0, longitude: 0 },
        { latitude: 0, longitude: 1 },
      ],
      'straight',
    );

    clickOnMap({ latitude: 0, longitude: 3 });
    expect(changes).toEqual([
      [
        { latitude: 0, longitude: 0 },
        { latitude: 0, longitude: 1 },
        { latitude: 0, longitude: 3 },
      ],
    ]);

    writeBack();
    expect(lastRoutePoints().map((point) => [point.lat, point.lng])).toEqual([
      [0, 0],
      [0, 1],
      [0, 3],
    ]);
    expect(lengths.at(-1)).toBeCloseTo(3 * ONE_DEGREE_KM, 6);
  });

  it('u izboru klik na pin u sredini niza uklanja ga, a pinovi posle njega dobijaju broj manji za jedan', () => {
    select(
      [
        { latitude: 0, longitude: 0 },
        { latitude: 0, longitude: 1 },
        { latitude: 0, longitude: 2 },
      ],
      'straight',
    );
    expect(pinNumbers()).toEqual(['1', '2', '3']);

    currentPins()[1].fire('click');
    expect(changes).toEqual([
      [
        { latitude: 0, longitude: 0 },
        { latitude: 0, longitude: 2 },
      ],
    ]);

    writeBack();
    expect(pinNumbers()).toEqual(['1', '2']);
    expect(currentPins()[1].getLatLng().lng).toBe(2);
  });

  it('tokom prevlačenja pina ne javlja niz i ne traži putanju, a posle puštanja menja samo taj pin', async () => {
    select(
      [
        { latitude: 0, longitude: 0 },
        { latitude: 0, longitude: 1 },
        { latitude: 0, longitude: 2 },
      ],
      'streets',
    );
    TestBed.tick();
    httpTesting
      .expectOne((request) => request.url.startsWith(WALKING_ROUTES_URL))
      .flush({
        points: [
          { latitude: 0, longitude: 0 },
          { latitude: 0, longitude: 2 },
        ],
        lengthInKilometres: 250,
      });
    await fixture.whenStable();
    const linesBeforeDrag = addedLines.mock.calls.length;

    const pin = currentPins()[1];
    pin.fire('dragstart');
    pin.setLatLng(L.latLng(0.5, 1.5));
    pin.fire('drag');
    fixture.detectChanges();
    TestBed.tick();
    expect(changes).toEqual([]);
    expect(walkingRouteRequests()).toBe(0);
    expect(addedLines.mock.calls.length).toBe(linesBeforeDrag);

    pin.fire('dragend');
    expect(changes).toEqual([
      [
        { latitude: 0, longitude: 0 },
        { latitude: 0.5, longitude: 1.5 },
        { latitude: 0, longitude: 2 },
      ],
    ]);
  });

  it('sa granicom 1 klik na praznu mapu premešta postojeći pin', () => {
    select([{ latitude: 0, longitude: 0 }], 'none', 1);

    clickOnMap({ latitude: 1, longitude: 1 });
    expect(changes).toEqual([[{ latitude: 1, longitude: 1 }]]);

    writeBack();
    expect(pinNumbers()).toEqual(['1']);
  });

  it('sa dostignutom granicom većom od 1 klik na praznu mapu ne dodaje pin i prikazuje poruku', () => {
    select(
      [
        { latitude: 0, longitude: 0 },
        { latitude: 0, longitude: 1 },
      ],
      'none',
      2,
    );
    expect(fixture.nativeElement.textContent).not.toContain(LIMIT_MESSAGE);

    clickOnMap({ latitude: 0, longitude: 2 });
    expect(changes).toEqual([]);
    expect(pinNumbers()).toEqual(['1', '2']);
    expect(fixture.nativeElement.textContent).toContain(LIMIT_MESSAGE);
  });

  it('početni niz prikazuje kao numerisane pinove, a klik na praznu mapu dodaje pin iza njih', () => {
    const start = [
      { latitude: 45.25, longitude: 19.84 },
      { latitude: 45.26, longitude: 19.85 },
      { latitude: 45.24, longitude: 19.86 },
    ];
    select(start, 'none');
    expect(pinNumbers()).toEqual(['1', '2', '3']);

    clickOnMap({ latitude: 45.27, longitude: 19.87 });
    expect(changes).toEqual([[...start, { latitude: 45.27, longitude: 19.87 }]]);

    writeBack();
    expect(pinNumbers()).toEqual(['1', '2', '3', '4']);
  });

  it('u postavljanju pozicije klik javlja poziciju i ne menja niz, a u biranju niza ne javlja poziciju', () => {
    const start = [
      { latitude: 0, longitude: 0 },
      { latitude: 0, longitude: 1 },
    ];

    offerPosition(start, true);
    clickOnMap({ latitude: 1, longitude: 1 });
    currentPins()[1].fire('click');
    currentPins()[0].fire('dragend');
    expect(positions).toEqual([{ latitude: 1, longitude: 1 }]);
    expect(changes).toEqual([]);
    expect(pinNumbers()).toEqual(['1', '2']);

    offerPosition(start, false);
    clickOnMap({ latitude: 2, longitude: 2 });
    expect(changes).toEqual([[...start, { latitude: 2, longitude: 2 }]]);
    expect(positions).toEqual([{ latitude: 1, longitude: 1 }]);
  });

  it('bez pozicije prikazuje poruku, a sa pozicijom pin drugačiji od pinova niza', () => {
    const start = [
      { latitude: 0, longitude: 0 },
      { latitude: 0, longitude: 1 },
    ];

    offerPosition(start, false, null, true);
    expect(fixture.nativeElement.textContent).toContain(NO_POSITION_MESSAGE);
    expect(count('.map-position')).toBe(0);

    offerPosition(start, false);
    expect(fixture.nativeElement.textContent).not.toContain(NO_POSITION_MESSAGE);
    expect(count('.map-position')).toBe(0);

    offerPosition(start, false, { latitude: 1, longitude: 1 });
    expect(fixture.nativeElement.textContent).not.toContain(NO_POSITION_MESSAGE);
    expect(count('.map-position')).toBe(1);
    expect(count('.map-position.map-pin')).toBe(0);
    expect(pinNumbers()).toEqual(['1', '2']);
  });
  it('kada se pomeri samo pin pozicije, zadržava pogled koji je korisnik namestio, a nov niz ponovo obuhvata', () => {
    const start = [
      { latitude: 0, longitude: 0 },
      { latitude: 0, longitude: 1 },
    ];
    offerPosition(start, true, { latitude: 1, longitude: 1 });
    const firstMap = addedTiles.mock.calls.at(-1)![0] as L.Map;
    firstMap.setView(L.latLng(0.5, 0.5), 7);

    fixture.componentRef.setInput('position', { latitude: 0.6, longitude: 0.6 });
    fixture.detectChanges();
    const secondMap = addedTiles.mock.calls.at(-1)![0] as L.Map;
    expect(secondMap).not.toBe(firstMap);
    expect(secondMap.getZoom()).toBe(7);
    expect(secondMap.getCenter()).toEqual(L.latLng(0.5, 0.5));

    fixture.componentRef.setInput('locations', [...start, { latitude: 0, longitude: 2 }]);
    fixture.detectChanges();
    const thirdMap = addedTiles.mock.calls.at(-1)![0] as L.Map;
    expect(thirdMap.getZoom()).not.toBe(7);
  });
});
