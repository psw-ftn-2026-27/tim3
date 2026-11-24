import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import * as L from 'leaflet';
import { MapPage } from './map';

const POSITION_URL = '/api/maps/user-positions/me';
const NO_POSITION_MESSAGE = 'Pozicija još nije postavljena.';
const NOT_SAVED_MESSAGE = 'Pozicija nije sačuvana.';

describe('MapPage', () => {
  let fixture: ComponentFixture<MapPage>;
  let httpTesting: HttpTestingController;
  let addedTiles: ReturnType<typeof vi.spyOn>;

  function text(): string {
    return fixture.nativeElement.textContent;
  }

  function count(selector: string): number {
    return fixture.nativeElement.querySelectorAll(selector).length;
  }

  // Stranica pri otvaranju čita sačuvanu poziciju; test odgovara umesto servera.
  async function open(response: { latitude: number; longitude: number } | 404): Promise<void> {
    fixture.detectChanges();
    TestBed.tick();
    const request = httpTesting.expectOne(POSITION_URL);
    expect(request.request.method).toBe('GET');
    if (response === 404) {
      request.flush(null, { status: 404, statusText: 'Not Found' });
    } else {
      request.flush({ ...response, setAt: '2026-01-01T12:00:00Z' });
    }
    await fixture.whenStable();
  }

  // setPosition čeka odgovor servera kao obično obećanje, koje whenStable ne prati, pa test
  // pušta da se ono završi pre nego što proveri stranicu.
  async function afterSave(): Promise<void> {
    await new Promise((resolve) => setTimeout(resolve));
    fixture.detectChanges();
    TestBed.tick();
  }

  function chooseSettingPosition(): void {
    const buttons = Array.from(
      fixture.nativeElement.querySelectorAll('button') as NodeListOf<HTMLButtonElement>,
    );
    buttons.find((button) => button.textContent!.trim() === 'Postavljanje pozicije')!.click();
    fixture.detectChanges();
  }

  // jsdom ne raspoređuje elemente, pa se klik na mapu izaziva događajem Leaflet-a.
  function clickOnMap(latitude: number, longitude: number): void {
    const map = addedTiles.mock.calls.at(-1)![0] as L.Map;
    map.fire('click', { latlng: L.latLng(latitude, longitude) });
    fixture.detectChanges();
  }

  function positionPin(): L.LatLng {
    const map = addedTiles.mock.calls.at(-1)![0] as L.Map;
    let found: L.LatLng | null = null;
    map.eachLayer((layer) => {
      if (layer instanceof L.Marker && layer.getElement()?.classList.contains('map-position')) {
        found = layer.getLatLng();
      }
    });
    return found!;
  }

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    httpTesting = TestBed.inject(HttpTestingController);
    addedTiles = vi.spyOn(L.TileLayer.prototype, 'onAdd');
    fixture = TestBed.createComponent(MapPage);
  });

  afterEach(() => {
    addedTiles.mockRestore();
    httpTesting.verify();
  });

  it('kada server javi da pozicija ne postoji, mapa kaže da pozicija još nije postavljena', async () => {
    await open(404);

    expect(text()).toContain(NO_POSITION_MESSAGE);
    expect(count('.map-position')).toBe(0);
  });

  it('kada čitanje pozicije ne uspe iz drugog razloga, mapa ne tvrdi da pozicija ne postoji', async () => {
    fixture.detectChanges();
    TestBed.tick();
    httpTesting
      .expectOne(POSITION_URL)
      .flush(null, { status: 500, statusText: 'Internal Server Error' });
    await fixture.whenStable();

    expect(text()).not.toContain(NO_POSITION_MESSAGE);
    expect(count('.map-position')).toBe(0);
  });

  it('klik u postavljanju pozicije šalje mesto serveru, a pin se premešta tek kada stranica ponovo pročita poziciju', async () => {
    await open({ latitude: 45.25, longitude: 19.84 });
    chooseSettingPosition();

    clickOnMap(45.26, 19.85);

    const save = httpTesting.expectOne(POSITION_URL);
    expect(save.request.method).toBe('PUT');
    expect(save.request.body).toEqual({ latitude: 45.26, longitude: 19.85 });
    expect(positionPin()).toEqual(L.latLng(45.25, 19.84));

    save.flush(null, { status: 204, statusText: 'No Content' });
    await afterSave();
    const reload = httpTesting.expectOne(POSITION_URL);
    expect(reload.request.method).toBe('GET');
    reload.flush({ latitude: 45.26, longitude: 19.85, setAt: '2026-01-01T12:05:00Z' });
    await fixture.whenStable();

    expect(positionPin()).toEqual(L.latLng(45.26, 19.85));
    expect(text()).not.toContain(NOT_SAVED_MESSAGE);
  });

  it('kada server ne sačuva poziciju, stranica to kaže, ne čita poziciju ponovo i pin ostaje na mestu', async () => {
    await open({ latitude: 45.25, longitude: 19.84 });
    chooseSettingPosition();

    clickOnMap(45.26, 19.85);
    httpTesting
      .expectOne(POSITION_URL)
      .flush(null, { status: 400, statusText: 'Bad Request' });
    await afterSave();

    expect(text()).toContain(NOT_SAVED_MESSAGE);
    httpTesting.expectNone(POSITION_URL);
    expect(positionPin()).toEqual(L.latLng(45.25, 19.84));
  });
});
