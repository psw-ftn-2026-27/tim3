# Mapa sa lokacijama

Komponenta `LocationMap` (`app-location-map`) je deljena prikazna komponenta platforme. Prikazuje mapu sa po jednim pinom na svakoj predatoj lokaciji, po želji i putanju između njih (pravim linijama ili po ulicama), i javlja dužinu te putanje. Po želji korisnik na njoj bira niz lokacija. Moduli je koriste na svojim stranicama, ali je ne menjaju. Primer upotrebe je platformska stranica `/map` (`core/map/`).

## Ulazi i izlaz

- `locations: MapLocation[]` je spisak lokacija, a `MapLocation` je `{ latitude: number; longitude: number }`. Tip se uvozi iz `location-map.ts`. Komponenta ne poznaje entitete modula, pa stranica svoje podatke (izazove, ključne tačke) sama prevodi u lokacije.
- `route: RouteKind` (podrazumevano `'none'`) bira vrstu prikaza putanje. `RouteKind` je `'none' | 'straight' | 'streets'` i uvozi se iz `location-map.ts`:
  - `'none'`: samo pinovi, bez putanje i bez dužine;
  - `'straight'`: putanja spaja susedne lokacije pravim linijama, redom kojim su predate;
  - `'streets'`: putanja prati pešačke ulice i staze između susednih lokacija, redom kojim su predate.
- `routeLength` je izlaz sa dužinom iscrtane putanje u kilometrima, kao brojem. Javlja se svaki put kada se putanja iscrta, pa i posle svake promene lokacija. Za prave linije to je dužina vazdušnom linijom, a za putanju po ulicama dužina po ulicama koju vrati server. Sa manje od dve lokacije linija se ne crta, a dužina je 0, i za prave linije i za putanju po ulicama. Bez putanje (`'none'`) dužina se ne javlja.
- `routeNotAlongStreets` je izlaz bez vrednosti. Javlja se kada je izabrana putanja po ulicama, a ona ne može da se dobije (vidi ispod).
- `selectable: boolean` (podrazumevano `false`) uključuje izbor niza lokacija (vidi ispod). Bez njega mapa samo prikazuje lokacije.
- `maxPins: number | null` (podrazumevano `null`, bez granice) je najveći broj pinova u izboru.
- `locationsChange` je izlaz sa celim novim nizom lokacija (`MapLocation[]`), redom. Javlja se samo u izboru.

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

## Prikaz pravim linijama i čitanje dužine

```ts
protected readonly routeLength = signal(0);
```

```html
<app-location-map
  [locations]="locations()"
  [route]="'straight'"
  (routeLength)="routeLength.set($event)"
/>
<p>Dužina putanje: {{ routeLength().toFixed(2) }} km</p>
```

Stranica odlučuje šta radi sa dužinom: prikazuje je, šalje je svom serveru ili je ne koristi.

## Prikaz putanje po ulicama

```html
<app-location-map
  [locations]="locations()"
  [route]="'streets'"
  (routeLength)="routeLength.set($event)"
  (routeNotAlongStreets)="notAlongStreets.set(true)"
/>
```

Komponenta sama pita server za pešačku putanju (`GET /api/maps/walking-routes`, modul Maps), samo dok je izabrana putanja po ulicama i ima bar dve lokacije. Dok čeka odgovor, prikazuje samo pinove i ne javlja dužinu. Kada odgovor stigne, crta liniju iz tačaka koje je vratio server i javlja dužinu po ulicama.

Isto važi i kada stranica promeni lokacije ili vrstu prikaza: dok čeka nov odgovor, komponenta ne javlja ništa, pa bi stranica i dalje prikazivala dužinu prethodne putanje ispod mape bez linije. Zato stranica briše svoju dužinu svaki put kada menja lokacije ili vrstu prikaza, na primer `routeLength.set(null)` uz signal tipa `number | null`, i prikazuje je samo kada nije `null`. Tako radi i stranica `/map`.

Kada server ne nađe put, ne odgovori ili odbije zahtev (na primer kada korisnik nije prijavljen), komponenta:

- crta celu putanju pravim linijama, nikada deo puta po ulicama;
- na samoj mapi prikazuje poruku „Putanja nije po ulicama: prikazane su prave linije.";
- javlja dužinu vazdušnom linijom kroz `routeLength`;
- javlja `routeNotAlongStreets`.

Stranica ne mora da obrađuje `routeNotAlongStreets`, jer poruku već prikazuje mapa. Signal je tu kada stranica želi još nešto da uradi, na primer da ne pošalje svom serveru dužinu koja nije po ulicama. Da li krajnji korisnik uopšte vidi putanju po ulicama i kada, odlučuje modul na svojoj stranici.

## Izbor niza lokacija

Sa `[selectable]="true"` korisnik bira niz lokacija na mapi:

- klik na praznu mapu dodaje pin na kraj niza;
- svaki pin prikazuje svoj redni broj u nizu, počev od 1;
- pin se prevlači na novo mesto, a ostali pinovi ostaju gde su;
- klik na pin ga uklanja; njegovi susedi se spajaju direktno, a pinovi posle njega dobijaju broj manji za jedan.

Putanja spaja pinove redom niza, u vrsti koju zada `route`, po istim pravilima kao gore. Tokom prevlačenja putanja se ne menja, `locationsChange` se ne javlja i ne šalje se zahtev za putanju po ulicama; sve to se dešava tek kada korisnik pusti pin.

Komponenta niz ne čuva. Posle svakog dodavanja, uklanjanja i puštanja prevučenog pina javlja ceo nov niz kroz `locationsChange`, a stranica mora da ga upiše u svoj signal i vrati mapi kroz `locations`. Ako stranica to ne uradi, mapa i dalje prikazuje stari niz. Početni niz (na primer lokacije entiteta koji se menja) stranica predaje kroz isti ulaz.

```ts
protected readonly locations = signal<MapLocation[]>([]);
```

```html
<app-location-map
  [locations]="locations()"
  [route]="'straight'"
  [selectable]="true"
  (locationsChange)="locations.set($event)"
/>
```

Granica broja pinova se zadaje ulazom `maxPins`:

- sa granicom 1, kada pin već postoji, klik na praznu mapu premešta taj pin;
- sa granicom većom od 1, kada je dostignuta, klik na praznu mapu ne dodaje pin, a na sredini mape se na kratko prikaže poruka „Uneli ste najveći dozvoljen broj pinova.".

Čuvanje niza je posao stranice, kao i provera najmanjeg potrebnog broja lokacija (na primer za turu) pri čuvanju. Da li niz sme da se menja odlučuje modul: ako ne sme, stranica ne uključuje `selectable`. Primer izbora sa sve tri granice je stranica `/map`.

## Pravila

- Mapu koristite samo kroz ulaze i izlaze opisane gore. Ne pristupajte Leaflet-u iz modula i ne dodajte ga u modul; nova mogućnost mape je zahtev platformskom timu.
- Lokacije komponenti predaje stranica, koja ih čita svojim `httpResource`-om. Jedino što komponenta sama čita sa servera je putanja po ulicama, kroz svoj `httpResource`. To je namerno odobren izuzetak od pravila da resurs postoji samo na stranici: tako je ceo postupak sa greškom (prave linije, poruka, vazdušna dužina, signal) na jednom mestu, a moduli koriste samo ulaze i izlaze. Izuzetak važi samo za platformske komponente u `shared/`; u modulima resurs i dalje postoji samo na stranici.
- Komponenta crta preko biblioteke Leaflet van stabla komponenti, pa koristi `effect()` i pristup svom elementu (`inject(ElementRef)`). To je odobren izuzetak od Angular podskupa, samo za ovu komponentu; ne prepisujte ga u module. HTML numerisanih pinova pravi Leaflet van šablona, pa stil komponente do njega ne stiže i pinovi su stilizovani inline, vrednostima iz tokena (`var(--color-accent)` i slično).
