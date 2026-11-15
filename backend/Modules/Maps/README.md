# Modul Maps

Modul Maps je platformski modul zadužen za pešačku putanju po ulicama i za trenutnu poziciju korisnika. Sastoji se od tri projekta. Projekat `Maps` sadrži sam modul, projekat `Maps.Contracts` kontrakt `IMapsApi` za čitanje pozicije iz drugih modula, a projekat `Maps.Tests` njegove testove. Studenti ovaj modul koriste kroz njegove krajnje tačke i kontrakt, ali ga ne menjaju i iz svojih modula referenciraju samo `Maps.Contracts`.

## Problem koji rešava

Više funkcionalnosti prikazuje putanju na mapi: tura, igra, klub. Prava linija između dve lokacije ne kaže kuda turista zaista ide ni koliko je put dug. Za putanju po ulicama treba spoljni servis za pronalaženje puta, a on ima svoja pravila korišćenja i svoj format odgovora. Kada bi svaki tim sam povezivao servis, svaki bi ga povezao drugačije, a zajednički servis bi dobijao više zahteva nego što dozvoljava.

Zato servis poziva jedan modul. On primi lokacije redom obilaska, pita servis za pešački put i vrati tačke linije i njenu dužinu u kilometrima. Modul ne poznaje nijedan entitet drugih modula: prima samo spisak lokacija, nikada turu ili igru.

Funkcionalnosti koje zavise od toga da je korisnik na mestu (E22, G21, G22) traže trenutnu poziciju korisnika. Dok izvor pozicije nije telefon, korisnik je postavlja klikom na mapu. Modul čuva poziciju i daje je drugim modulima tako da oni ne znaju kako je postavljena, pa izvor pozicije može da se zameni bez izmene modula koji je čitaju.

## Pravila

Modul vraća putanju samo kada važi sledeće:

- zahtev šalje prijavljen korisnik, bilo koje uloge;
- zahtev nosi bar dve lokacije;
- servis za pronalaženje puta odgovori i nađe pešački put za svaki deo putanje.

Kada servis ne nađe put za bilo koji deo, ne odgovori ili odbije zahtev (na primer zbog prekoračenja, HTTP 429), upit završava greškom. Modul nikada ne vraća praznu ili delimičnu putanju.

Spoljni servis je javni OSRM server `https://routing.openstreetmap.de` sa pešačkim profilom, bez naloga i ključa. Njegovi uslovi korišćenja dozvoljavaju najviše jedan zahtev u sekundi i traže atribuciju OpenStreetMap-a, koju već prikazuje deljena mapa. Zato modul sve lokacije šalje u jednom zahtevu, nikada po jedan zahtev za svaki deo puta.

OSRM server prihvata samo vezu preko TLS 1.3. Na macOS-u .NET TLS 1.3 podržava samo kroz Apple Network.framework, pa `Host.Api/Properties/launchSettings.json` postavlja promenljivu `DOTNET_SYSTEM_NET_SECURITY_USENETWORKFRAMEWORK=1`. Ko server pokreće bez tog profila, na macOS-u uvek dobija prave linije, jer veza sa OSRM-om ne uspeva.

## Pravila pozicije

- Server čuva tačno jednu trenutnu poziciju po korisniku: geografsku širinu, dužinu, korisnika i vreme postavljanja. Nova pozicija zamenjuje prethodnu, a istorija se ne čuva.
- Prijavljen korisnik, bilo koje uloge, postavlja samo sopstvenu poziciju; korisnik stiže iz prijave, a ne iz zahteva. Vreme postavljanja daje server.
- Širina mora biti između −90 i 90, a dužina između −180 i 180.
- Pozicija važi dok korisnik ne postavi novu. Vreme postavljanja se čuva da bi modul koji je čita sam odlučio o svežini.
- Modul Maps ne odlučuje da li je korisnik blizu nečega. Pravila blizine pripadaju modulu koji poseduje posledicu.

## Krajnje tačke

- `GET /api/maps/walking-routes?locations[0].latitude=45.2671&locations[0].longitude=19.8335&locations[1].latitude=45.2551&locations[1].longitude=19.8451` prima lokacije redom obilaska i vraća `{ "points": [{ "latitude": ..., "longitude": ... }], "lengthInKilometres": ... }`. Kada zahtev nosi manje od dve lokacije, vraća odgovor sa statusnim kodom 400. Kada servis ne nađe put, vraća odgovor sa statusnim kodom 404. Kada servis ne odgovori ili odbije zahtev, vraća odgovor sa statusnim kodom 500.
- `PUT /api/maps/user-positions/me` sa telom `{ "latitude": 45.2551, "longitude": 19.8451 }` postavlja poziciju prijavljenog korisnika i vraća odgovor sa statusnim kodom 204. Kada su koordinate van dozvoljenog opsega, vraća odgovor sa statusnim kodom 400.
- `GET /api/maps/user-positions/me` vraća poziciju prijavljenog korisnika, `{ "latitude": ..., "longitude": ..., "setAt": "..." }`. Kada korisnik još nije postavio poziciju, vraća odgovor sa statusnim kodom 404, nikada podrazumevanu lokaciju.

Obe krajnje tačke pozicije služe samo stranici na kojoj korisnik postavlja poziciju. Moduli poziciju nikada ne čitaju kroz njih, već kroz kontrakt.

## Kontrakt

`Maps.Contracts` sadrži interfejs `IMapsApi` sa metodom `GetCurrentPositionAsync(Guid userId)`, koja vraća `CurrentPosition` (širina, dužina i vreme postavljanja) ili `null` kada korisnik nema poziciju. Kontrakt ne vraća podrazumevanu lokaciju ni izvor pozicije. Modul koji čita poziciju referencira `Maps.Contracts` iz svog `Application` projekta i ubrizgava `IMapsApi`, kao i kontrakt bilo kog drugog modula; svaka izmena kontrakta dogovor je među timovima.

## Kako modul koristi putanju

1. Stranica modula prosledi lokacije deljenoj komponenti mape i izabere prikaz po ulicama.
2. Komponenta mape pošalje lokacije na `GET /api/maps/walking-routes` i iscrta liniju iz vraćenih tačaka.
3. Kada upit završi greškom, komponenta crta prave linije i javlja stranici da putanja nije po ulicama.

Modul nikada ne referencira projekat `Maps` i ne poziva njegove klase, već samo `Maps.Contracts`. Arhitektonski testovi u `Host.Tests` proveravaju da nijedan modul ne zavisi od projekta `Maps` i da `Maps.Contracts` ne zavisi ni od čega.

## Odstupanje od strukture modula

Kao i Identity, modul ima jedan projekat sa direktorijumima `Api`, `Core` i `Infrastructure`, umesto pet projekata, i poseban projekat `Maps.Contracts` za kontrakt. Putanja se ne čuva: svaka se traži iznova od spoljnog servisa. Pozicija se čuva u sopstvenoj šemi `maps`.

- `Core` sadrži vrednosni objekat `RouteLocations`, koji u konstruktoru proverava da putanja ima bar dve lokacije, grupu slučajeva korišćenja `WalkingRoutes` (klasa `WalkingRoutesQueries` i DTO strukture `LocationDto` i `WalkingRouteDto`) i interfejs `IRoutingService` za spoljni servis za pronalaženje puta.
- `Infrastructure` sadrži adapter `OsrmRoutingService`, koji kroz `HttpClient` šalje sve lokacije OSRM serveru u jednom zahtevu i njegov odgovor prevodi u `WalkingRouteDto`, a metre u kilometre. Samo OSRM-ov odgovor da puta nema (kod `NoRoute` ili `NoSegment`) prevodi u „put nije nađen"; svaki drugi neuspeh servisa ostaje izuzetak.
- `Core` sadrži i agregat `UserPosition` (jedan po korisniku, sa metodom `MoveTo` za novo postavljanje), vrednosni objekat `Coordinates`, koji u konstruktoru proverava opseg širine i dužine, interfejse `IUserPositionRepository`, `IUserPositionReadRepository` i `IUnitOfWork`, grupu slučajeva korišćenja `UserPositioning` (klase `UserPositioningService` i `UserPositioningQueries` i DTO strukture `SetUserPositionDto` i `UserPositionDto`) i klasu `MapsApi`, koja implementira `IMapsApi` kroz repozitorijum za čitanje.
- `Infrastructure/Persistence/` sadrži `MapsDbContext` na šemi `maps`, EF konfiguraciju i repozitorijume pozicije, migracije i `MapsModuleInitializer`, koji pri pokretanju primenjuje migracije. Migracije se dodaju komandom `dotnet ef migrations add <Ime> --project Modules/Maps/Maps --startup-project Host.Api --context MapsDbContext --output-dir Infrastructure/Persistence/Migrations`.
- `Api` sadrži kontrolere `WalkingRoutesController` i `UserPositioningController`.

Javni OSRM server je spoljna zavisnost bez testne instance, pa ga integracioni testovi ne pozivaju. `MapsApiFactory` umesto pravog adaptera registruje ručno napisanu lažnu klasu `FakeRoutingService` iz `Maps.Tests`, koja vraća unapred zadatu putanju, a za jednu zadatu lokaciju javlja da put ne postoji.
