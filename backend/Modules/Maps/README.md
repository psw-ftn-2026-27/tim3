# Modul Maps

Modul Maps je platformski modul zadužen za pešačku putanju po ulicama. Sastoji se od dva projekta. Projekat `Maps` sadrži sam modul, a projekat `Maps.Tests` njegove testove. Studenti ovaj modul koriste kroz njegovu krajnju tačku, ali ga ne menjaju i ne referenciraju iz svojih modula.

## Problem koji rešava

Više funkcionalnosti prikazuje putanju na mapi: tura, igra, klub. Prava linija između dve lokacije ne kaže kuda turista zaista ide ni koliko je put dug. Za putanju po ulicama treba spoljni servis za pronalaženje puta, a on ima svoja pravila korišćenja i svoj format odgovora. Kada bi svaki tim sam povezivao servis, svaki bi ga povezao drugačije, a zajednički servis bi dobijao više zahteva nego što dozvoljava.

Zato servis poziva jedan modul. On primi lokacije redom obilaska, pita servis za pešački put i vrati tačke linije i njenu dužinu u kilometrima. Modul ne poznaje nijedan entitet drugih modula: prima samo spisak lokacija, nikada turu ili igru.

## Pravila

Modul vraća putanju samo kada važi sledeće:

- zahtev šalje prijavljen korisnik, bilo koje uloge;
- zahtev nosi bar dve lokacije;
- servis za pronalaženje puta odgovori i nađe pešački put za svaki deo putanje.

Kada servis ne nađe put za bilo koji deo, ne odgovori ili odbije zahtev (na primer zbog prekoračenja, HTTP 429), upit završava greškom. Modul nikada ne vraća praznu ili delimičnu putanju.

Spoljni servis je javni OSRM server `https://routing.openstreetmap.de` sa pešačkim profilom, bez naloga i ključa. Njegovi uslovi korišćenja dozvoljavaju najviše jedan zahtev u sekundi i traže atribuciju OpenStreetMap-a, koju već prikazuje deljena mapa. Zato modul sve lokacije šalje u jednom zahtevu, nikada po jedan zahtev za svaki deo puta.

OSRM server prihvata samo vezu preko TLS 1.3. Na macOS-u .NET TLS 1.3 podržava samo kroz Apple Network.framework, pa `Host.Api/Properties/launchSettings.json` postavlja promenljivu `DOTNET_SYSTEM_NET_SECURITY_USENETWORKFRAMEWORK=1`. Ko server pokreće bez tog profila, na macOS-u uvek dobija prave linije, jer veza sa OSRM-om ne uspeva.

## Krajnja tačka

- `GET /api/maps/walking-routes?locations[0].latitude=45.2671&locations[0].longitude=19.8335&locations[1].latitude=45.2551&locations[1].longitude=19.8451` prima lokacije redom obilaska i vraća `{ "points": [{ "latitude": ..., "longitude": ... }], "lengthInKilometres": ... }`. Kada zahtev nosi manje od dve lokacije, vraća odgovor sa statusnim kodom 400. Kada servis ne nađe put, vraća odgovor sa statusnim kodom 404. Kada servis ne odgovori ili odbije zahtev, vraća odgovor sa statusnim kodom 500.

## Kako modul koristi putanju

1. Stranica modula prosledi lokacije deljenoj komponenti mape i izabere prikaz po ulicama.
2. Komponenta mape pošalje lokacije na `GET /api/maps/walking-routes` i iscrta liniju iz vraćenih tačaka.
3. Kada upit završi greškom, komponenta crta prave linije i javlja stranici da putanja nije po ulicama.

Modul nikada ne referencira projekat `Maps` i ne poziva njegove klase. Arhitektonski test to proverava.

## Odstupanje od strukture modula

Kao i Media, modul ima jedan projekat sa direktorijumima `Api`, `Core` i `Infrastructure`, umesto pet projekata. Nema bazu podataka i ništa ne čuva: svaka putanja se traži iznova od spoljnog servisa.

- `Core` sadrži vrednosni objekat `RouteLocations`, koji u konstruktoru proverava da putanja ima bar dve lokacije, grupu slučajeva korišćenja `WalkingRoutes` (klasa `WalkingRoutesQueries` i DTO strukture `LocationDto` i `WalkingRouteDto`) i interfejs `IRoutingService` za spoljni servis za pronalaženje puta.
- `Infrastructure` sadrži adapter `OsrmRoutingService`, koji kroz `HttpClient` šalje sve lokacije OSRM serveru u jednom zahtevu i njegov odgovor prevodi u `WalkingRouteDto`, a metre u kilometre. Samo OSRM-ov odgovor da puta nema (kod `NoRoute` ili `NoSegment`) prevodi u „put nije nađen"; svaki drugi neuspeh servisa ostaje izuzetak.
- `Api` sadrži kontroler `WalkingRoutesController`.

Javni OSRM server je spoljna zavisnost bez testne instance, pa ga integracioni testovi ne pozivaju. `MapsApiFactory` umesto pravog adaptera registruje ručno napisanu lažnu klasu `FakeRoutingService` iz `Maps.Tests`, koja vraća unapred zadatu putanju, a za jednu zadatu lokaciju javlja da put ne postoji.
