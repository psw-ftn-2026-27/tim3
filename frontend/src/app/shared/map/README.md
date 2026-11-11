# Mapa sa lokacijama

Komponenta `LocationMap` (`app-location-map`) je deljena prikazna komponenta platforme. Prikazuje mapu sa po jednim pinom na svakoj predatoj lokaciji, po želji i putanju između njih, i javlja dužinu te putanje. Moduli je koriste na svojim stranicama, ali je ne menjaju. Primer upotrebe je platformska stranica `/map` (`core/map/`).

## Ulazi i izlaz

- `locations: MapLocation[]` je spisak lokacija, a `MapLocation` je `{ latitude: number; longitude: number }`. Tip se uvozi iz `location-map.ts`. Komponenta ne poznaje entitete modula, pa stranica svoje podatke (izazove, ključne tačke) sama prevodi u lokacije.
- `showRoute: boolean` (podrazumevano `false`) traži putanju. Putanja spaja susedne lokacije pravim linijama, redom kojim su predate.
- `routeLength` je izlaz sa dužinom putanje u kilometrima, kao brojem. Javlja se svaki put kada se putanja iscrta, pa i posle svake promene lokacija. Sa manje od dve lokacije linija se ne crta, a dužina je 0.

Mapa uvek obuhvata sve pinove. Bez ijedne lokacije prikazuje Novi Sad. Podloga je OpenStreetMap, sa navodom autora u uglu mape. Visina mape je 24rem.

## Prikaz bez putanje

```ts
import { LocationMap, MapLocation } from '../../../../shared/map/location-map/location-map';

// u komponenti stranice, uz imports: [LocationMap]
protected readonly locations = computed<MapLocation[]>(() =>
  (this.keyPoints.value() ?? []).map((point) => ({ latitude: point.latitude, longitude: point.longitude })),
);
```

```html
<app-location-map [locations]="locations()" />
```

## Prikaz sa putanjom i čitanje dužine

```ts
protected readonly routeLength = signal(0);
```

```html
<app-location-map
  [locations]="locations()"
  [showRoute]="true"
  (routeLength)="routeLength.set($event)"
/>
<p>Dužina putanje: {{ routeLength().toFixed(2) }} km</p>
```

Stranica odlučuje šta radi sa dužinom: prikazuje je, šalje je svom serveru ili je ne koristi.

## Pravila

- Mapu koristite samo kroz ova dva ulaza i izlaz. Ne pristupajte Leaflet-u iz modula i ne dodajte ga u modul; nova mogućnost mape je zahtev platformskom timu.
- Komponenta ništa ne čita sa servera. Lokacije joj predaje stranica, koja ih čita svojim `httpResource`-om.
- Komponenta crta preko biblioteke Leaflet van stabla komponenti, pa koristi `effect()` i pristup svom elementu (`inject(ElementRef)`). To je odobren izuzetak od Angular podskupa, samo za ovu komponentu; ne prepisujte ga u module.
