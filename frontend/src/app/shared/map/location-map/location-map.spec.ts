import { ComponentFixture, TestBed } from '@angular/core/testing';
import { LocationMap, MapLocation } from './location-map';

// Jedan stepen geografske dužine na ekvatoru, mereno po površini Zemlje (poluprečnik 6371 km).
const ONE_DEGREE_KM = (Math.PI * 6371) / 180;

describe('LocationMap', () => {
  let fixture: ComponentFixture<LocationMap>;
  let lengths: number[];

  function show(locations: MapLocation[], showRoute: boolean): void {
    fixture.componentRef.setInput('locations', locations);
    fixture.componentRef.setInput('showRoute', showRoute);
    fixture.detectChanges();
  }

  function count(selector: string): number {
    return fixture.nativeElement.querySelectorAll(selector).length;
  }

  beforeEach(() => {
    fixture = TestBed.createComponent(LocationMap);
    lengths = [];
    fixture.componentInstance.routeLength.subscribe((length) => lengths.push(length));
  });

  it('sabira rastojanja susednih lokacija redom kojim su predate', () => {
    show(
      [
        { latitude: 0, longitude: 0 },
        { latitude: 0, longitude: 1 },
        { latitude: 0, longitude: 3 },
      ],
      true,
    );
    expect(lengths.at(-1)).toBeCloseTo(3 * ONE_DEGREE_KM, 6);

    show(
      [
        { latitude: 0, longitude: 0 },
        { latitude: 0, longitude: 3 },
        { latitude: 0, longitude: 1 },
      ],
      true,
    );
    expect(lengths.at(-1)).toBeCloseTo(5 * ONE_DEGREE_KM, 6);
  });

  it('sa jednom lokacijom ili bez lokacija daje 0 km i ne crta liniju', () => {
    show([{ latitude: 45.25, longitude: 19.85 }], true);
    expect(lengths.at(-1)).toBe(0);
    expect(count('.map-route')).toBe(0);

    show([], true);
    expect(lengths.at(-1)).toBe(0);
    expect(count('.map-route')).toBe(0);
  });

  it('prikazuje po jedan pin za svaku lokaciju, a putanju samo kada je zatražena', () => {
    const locations = [
      { latitude: 45.25, longitude: 19.84 },
      { latitude: 45.26, longitude: 19.85 },
      { latitude: 45.24, longitude: 19.86 },
    ];

    show(locations, false);
    expect(count('.map-pin')).toBe(3);
    expect(count('.map-route')).toBe(0);
    expect(lengths).toEqual([]);

    show(locations, true);
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
      true,
    );
    expect(count('.map-pin')).toBe(3);
    expect(lengths.at(-1)).toBeCloseTo(2 * ONE_DEGREE_KM, 6);

    show(
      [
        { latitude: 0, longitude: 0 },
        { latitude: 0, longitude: 4 },
      ],
      true,
    );
    expect(count('.map-pin')).toBe(2);
    expect(lengths.at(-1)).toBeCloseTo(4 * ONE_DEGREE_KM, 6);
  });
});
