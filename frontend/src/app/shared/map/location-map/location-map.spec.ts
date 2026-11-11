import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import * as L from 'leaflet';
import { LocationMap, MapLocation, RouteKind } from './location-map';

// Jedan stepen geografske dužine na ekvatoru, mereno po površini Zemlje (poluprečnik 6371 km).
const ONE_DEGREE_KM = (Math.PI * 6371) / 180;

const WALKING_ROUTES_URL = '/api/maps/walking-routes';
const MESSAGE = 'Putanja nije po ulicama: prikazane su prave linije.';

describe('LocationMap', () => {
  let fixture: ComponentFixture<LocationMap>;
  let httpTesting: HttpTestingController;
  let lengths: number[];
  let notAlongStreets: number;
  let addedLines: ReturnType<typeof vi.spyOn>;

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

  function walkingRouteRequests(): number {
    return httpTesting.match((request) => request.url.startsWith(WALKING_ROUTES_URL)).length;
  }

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    httpTesting = TestBed.inject(HttpTestingController);
    addedLines = vi.spyOn(L.Polyline.prototype, 'onAdd');
    fixture = TestBed.createComponent(LocationMap);
    lengths = [];
    notAlongStreets = 0;
    fixture.componentInstance.routeLength.subscribe((length) => lengths.push(length));
    fixture.componentInstance.routeNotAlongStreets.subscribe(() => notAlongStreets++);
  });

  afterEach(() => {
    addedLines.mockRestore();
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

    httpTesting.expectOne((request) => request.url.startsWith(WALKING_ROUTES_URL)).flush({
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
});
