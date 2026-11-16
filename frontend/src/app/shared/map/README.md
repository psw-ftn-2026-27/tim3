# Mapa sa lokacijama

Komponenta `LocationMap` (`app-location-map`) je deljena prikazna komponenta platforme. Prikazuje mapu sa po jednim pinom na svakoj predatoj lokaciji, po želji i putanju između njih (pravim linijama ili po ulicama), i javlja dužinu te putanje. Po želji prikazuje i krug oko zadatog centra. Po želji korisnik na njoj bira niz lokacija, centar kruga ili jedan od prikazanih pinova. Moduli je koriste na svojim stranicama, ali je ne menjaju. Primer upotrebe je platformska stranica `/map` (`core/map/`).

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
- `positionSettable: boolean` (podrazumevano `false`) nudi postavljanje pozicije korisnika (vidi ispod). Bez njega mapa ne nudi postavljanje pozicije.
- `settingPosition: boolean` (podrazumevano `false`) je režim klika: dok je uključen, klik na mapu postavlja poziciju umesto da menja niz. Važi samo uz `positionSettable`.
- `position: MapLocation | null` (podrazumevano `null`) je sačuvana pozicija korisnika, prikazana posebnim pinom. Pin se prikazuje i bez `positionSettable`, pa mapa koja poziciju samo pokazuje (na primer za E22 i G22) predaje samo ovaj ulaz.
- `positionMissing: boolean` (podrazumevano `false`) je tačan kada server javi da korisnik još nije postavio poziciju. Uz `positionSettable` mapa tada prikazuje poruku da pozicija još nije postavljena.
- `positionSet` je izlaz sa mestom klika (`MapLocation`) u režimu postavljanja pozicije.
- `circle: MapCircle | null` (podrazumevano `null`, bez kruga) je krug koji mapa crta (vidi ispod). `MapCircle` je `{ center: MapLocation; radiusInMetres: number }` i uvozi se iz `location-map.ts`.
- `pickingCircle: boolean` (podrazumevano `false`) je režim klika: dok je uključen, klik na mapu bira centar kruga umesto da menja niz.
- `circleCenterSet` je izlaz sa mestom klika (`MapLocation`) u izboru kruga.
- `pickingPin: boolean` (podrazumevano `false`) uključuje izbor pina (vidi ispod): klik na pin ga ističe.
- `pinPicked` je izlaz sa indeksom (`number`) izabrane lokacije u nizu `locations` koji je stranica predala. Javlja se samo u izboru pina.

Mapa uvek obuhvata sve pinove, i pin pozicije, i ceo krug. Izuzetak je pomeranje pina pozicije koji je već bio na mapi: korisnik ga postavlja klikom na deo mape koji vidi, pa mapa tada zadržava pogled (pomeranje i zum) koji je korisnik namestio. Bez ijedne lokacije prikazuje Novi Sad. Podloga je OpenStreetMap, sa navodom autora u uglu mape. Visina mape je 24rem.

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

## Postavljanje pozicije

Funkcionalnosti koje zavise od toga da je korisnik na mestu traže njegovu trenutnu poziciju. Dok izvor pozicije nije telefon, korisnik je postavlja klikom na mapu. Da li mapa nudi postavljanje pozicije odlučuje stranica; primer je platformska stranica `/map`.

Sačuvanu poziciju iz ulaza `position` mapa uvek prikazuje kao zeleni kvadrat bez broja, drugačiji od pinova lokacija; pin pozicije se ne prevlači i ne uklanja klikom. Sa `[positionSettable]="true"` mapa uz to:

- kada je `positionMissing` tačan, na mapi prikazuje poruku „Pozicija još nije postavljena.";
- dok je `settingPosition` uključen, klik na mapu javlja mesto klika kroz `positionSet`, a pinovi niza se ne dodaju, ne pomeraju i ne uklanjaju;
- dok `settingPosition` nije uključen, klik radi kao i bez postavljanja pozicije.

Komponenta poziciju ne čuva. Stranica mesto klika šalje serveru (`PUT /api/maps/user-positions/me`, modul Maps), a kada ga server sačuva, ponovo čita poziciju (`GET /api/maps/user-positions/me`) i vraća je mapi kroz `position`. Server vraća 404 dok korisnik nije postavio poziciju; samo tada stranica predaje `positionMissing` kao tačno. Dok se pozicija učitava ili kada čitanje ne uspe iz drugog razloga (na primer 401 ili 500), stranica predaje `position` kao `null` i `positionMissing` kao netačno, pa mapa ne crta pin i ne prikazuje poruku. Pin se zato pojavljuje ili premešta tek kada server sačuva poziciju, a ponovni ulazak na stranicu prikazuje istu poziciju.

```html
<app-location-map
  [locations]="locations()"
  [selectable]="true"
  [positionSettable]="true"
  [settingPosition]="settingPosition()"
  [position]="position()"
  [positionMissing]="positionMissing()"
  (locationsChange)="locations.set($event)"
  (positionSet)="setPosition($event)"
/>
```

Režim klika bira stranica, na `/map` dugmadima „Biranje niza", „Postavljanje pozicije", „Biranje kruga" i „Biranje pina".

## Krug i izbor centra

Krug pokazuje oblast, na primer oblast pretrage tura (E23) ili oblast traženja skrivene lokacije (G21). Mapa ga crta samo kada stranica preda `circle`, sa centrom i poluprečnikom u metrima. Krug obuhvata tačke čije rastojanje od centra, mereno po površini Zemlje, nije veće od poluprečnika. Crta se bojom akcenta, kao putanja, i ne prima klik, pa klik unutar kruga ide mapi.

Poluprečnik zadaje stranica, a komponenta crta ono što dobije. Kada stranica promeni poluprečnik, krug se ponovo iscrta oko istog centra. Stranica poluprečnik prikazuje u jedinici koja joj odgovara (na primer km), ali ga mapi predaje u metrima. Ispravnost poluprečnika proverava stranica; dok nije ispravan, ona mapi predaje `circle` kao `null`.

Sa `[pickingCircle]="true"` klik na praznu mapu javlja mesto klika kroz `circleCenterSet`, a pinovi niza se ne dodaju, ne pomeraju i ne uklanjaju. Dok je uključeno postavljanje pozicije (`positionSettable` i `settingPosition`), ono ima prednost, pa klik javlja poziciju, a ne centar. Postoji najviše jedan centar: svaki sledeći klik javlja nov centar, a stranica ga upisuje u svoj signal umesto starog.

Komponenta centar ne čuva. Stranica ga upisuje u svoj signal i vraća mapi kroz `circle`; ako to ne uradi, mapa ne crta krug. Da bi uklonila krug, stranica obriše svoj centar i preda `null`.

```ts
protected readonly circleCenter = signal<MapLocation | null>(null);
protected readonly circle = computed<MapCircle | null>(() => {
  const center = this.circleCenter();
  return center === null ? null : { center, radiusInMetres: this.radius() };
});
```

```html
<app-location-map
  [locations]="locations()"
  [pickingCircle]="true"
  [circle]="circle()"
  (circleCenterSet)="circleCenter.set($event)"
/>
```

Koje su tačke ili entiteti u krugu mapa ne javlja, jer ne poznaje entitete modula; to odlučuje server modula (na primer Exploration u lokacijskoj pretrazi tura). Na stranici `/map` režim klika bira se dugmadima „Biranje niza", „Postavljanje pozicije", „Biranje kruga" i „Biranje pina", pa je uvek uključen tačno jedan; poluprečnik se zadaje poljem „Poluprečnik (m)", a krug se uklanja dugmetom „Ukloni krug".

## Izbor pina

Izbor pina služi da korisnik izabere jedan od prikazanih entiteta klikom na njegov pin, na primer izazov u G22 ili turu u E23. Sa `[pickingPin]="true"`:

- pinovi se prikazuju bez rednih brojeva;
- klik na pin ga ističe (veći je, sa debljim okvirom u boji akcenta) i javlja indeks te lokacije kroz `pinPicked`; istaknut je samo poslednji izabran pin;
- klik na pin ne menja lokacije i ne uklanja pin, pa se `locationsChange` ne javlja;
- klik na praznu mapu ne menja istaknut pin i ne javlja ništa;
- kada stranica preda nove lokacije (nov niz u `locations`, na primer posle nove pretrage), nijedan pin nije istaknut.

Kada više entiteta ima istu lokaciju, pinovi se ne razdvajaju, a bira se pin koji je na vrhu.

Istaknut pin drži komponenta. Izabran entitet drži stranica: mapa javlja indeks u nizu koji je stranica predala, a ne koordinate, jer je indeks jednoznačan i kada dva entiteta imaju istu lokaciju. Stranica lokacije pravi iz svojih entiteta istim redom, pa preko indeksa nalazi entitet. Komponenta entitete ne poznaje; šta stranica prikaže posle izbora (detalje izazova, aktivaciju) odlučuje modul. Mapa briše istaknut pin čim dobije nov niz, pa stranica briše svoj izabran indeks na svakom mestu gde pokreće nove lokacije (nova pretraga, ponovno čitanje resursa); inače bi indeks pokazivao na drugi entitet u novom spisku. Istaknut pin se ne briše kada stranica preda isti niz, pa stranica koja briše svoj indeks predaje i nov niz (na primer kopiju, `[...LOCATIONS]`).

```ts
protected readonly challenges = computed(() => this.challengesResource.value() ?? []);
protected readonly locations = computed<MapLocation[]>(() =>
  this.challenges().map((challenge) => ({ latitude: challenge.latitude, longitude: challenge.longitude })),
);
protected readonly pickedIndex = signal<number | null>(null);
protected readonly pickedChallenge = computed(() => {
  const index = this.pickedIndex();
  return index === null ? null : this.challenges()[index];
});

// Nova pretraga donosi nov spisak, pa stari indeks više ne važi.
protected search(): void {
  this.pickedIndex.set(null);
  this.challengesResource.reload();
}
```

```html
<app-location-map
  [locations]="locations()"
  [pickingPin]="true"
  (pinPicked)="pickedIndex.set($event)"
/>
```

Izbor pina i izbor niza ne uključuju se zajedno na istoj mapi. Ako stranica ipak uključi oba (`selectable` i `pickingPin`), izbor niza ima prednost: pinovi su numerisani i indeks se ne javlja, a klik na pin ga uklanja kao i u izboru niza dok nije uključen ni izbor kruga ni postavljanje pozicije. Prednost ima i postavljanje pozicije: dok je uključeno (`positionSettable` i `settingPosition`), klik na pin ne javlja indeks. Izbor pina i izbor kruga mogu zajedno, kako ih koristi E23: klik na praznu mapu javlja centar kruga, a klik na pin javlja indeks i ne javlja centar.

Na stranici `/map` izbor pina se uključuje dugmetom „Biranje pina"; ispod mape piše izabrana lokacija ili „Pin nije izabran.", a dugme „Druge lokacije" glumi novu pretragu.

## Čitanje pozicije iz modula

Modul poziciju nikada ne čita kroz krajnje tačke pozicije, jer one služe samo stranici na kojoj se pozicija postavlja. Server modula je čita kroz kontrakt `IMapsApi` iz `Maps.Contracts` (vidi `backend/Modules/Maps/README.md`), koji vraća širinu, dužinu i vreme postavljanja ili odgovor da pozicija ne postoji. Da li je korisnik blizu nečega odlučuje modul, ne mapa.

## Koju mogućnost mape koristi koja funkcionalnost

| Funkcionalnost | Mogućnost mape |
|---|---|
| E21, ključne tačke ture | Izbor niza lokacija (`selectable`), gde je svaka ključna tačka jedan pin; putanja između tačaka (`route`) i njena dužina iz izlaza `routeLength`. |
| E22, obilazak ture | Pozicija korisnika (`positionSettable`, `position`) i putanja od pozicije do sledeće ključne tačke: stranica preda poziciju i tačku kao dve lokacije u `locations` i izabere `route`. Proveru na svakih 10 sekundi i pravilo blizine radi stranica, odnosno server modula kroz `IMapsApi`. |
| E23, pretraga tura | Prikaz pinova tura (`locations`) i krug oblasti pretrage (`circle`), čiji centar korisnik bira klikom (`pickingCircle`, `circleCenterSet`). Koje su ture u krugu odlučuje server Exploration-a. Izbor ture klikom na njen pin (`pickingPin`, `pinPicked`), uz izbor kruga na istoj mapi. |
| G21, skrivena lokacija | Pozicija korisnika (`positionSettable`, `position`); blizinu proverava server modula kroz `IMapsApi`. Krug oblasti traženja (`circle`), čiji poluprečnik zadaje Games. |
| G22, izazov na mapi | Pozicija korisnika (`positionSettable`, `position`) i prikaz pinova izazova (`locations`). Proveru na svakih 10 sekundi radi stranica. Izbor izazova klikom na njegov pin (`pickingPin`, `pinPicked`). |

## Pravila

- Mapu koristite samo kroz ulaze i izlaze opisane gore. Ne pristupajte Leaflet-u iz modula i ne dodajte ga u modul; nova mogućnost mape je zahtev platformskom timu.
- Lokacije komponenti predaje stranica, koja ih čita svojim `httpResource`-om. Jedino što komponenta sama čita sa servera je putanja po ulicama, kroz svoj `httpResource`. To je namerno odobren izuzetak od pravila da resurs postoji samo na stranici: tako je ceo postupak sa greškom (prave linije, poruka, vazdušna dužina, signal) na jednom mestu, a moduli koriste samo ulaze i izlaze. Izuzetak važi samo za platformske komponente u `shared/`; u modulima resurs i dalje postoji samo na stranici.
- Komponenta crta preko biblioteke Leaflet van stabla komponenti, pa koristi `effect()` i pristup svom elementu (`inject(ElementRef)`). To je odobren izuzetak od Angular podskupa, samo za ovu komponentu; ne prepisujte ga u module. HTML numerisanih pinova i pinova u izboru pina pravi Leaflet van šablona, pa stil komponente do njega ne stiže i pinovi su stilizovani inline, vrednostima iz tokena (`var(--color-accent)` i slično).
