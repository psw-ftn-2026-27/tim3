# Modul Media

Modul Media je platformski modul zadužen za prijem, proveru i čuvanje slika. Sastoji se od dva projekta. Projekat `Media` sadrži sam modul, a projekat `Media.Tests` njegove testove. Studenti ovaj modul koriste kroz njegove krajnje tačke, ali ga ne menjaju i ne referenciraju iz svojih modula.

## Problem koji rešava

Više funkcionalnosti traži sliku: blog, tura, klub. Prijem fajla od korisnika traži niz provera, jer fajl od nepoznatog korisnika može da bude prevelik, da ne bude slika ili da ima ime kojim se napada server. Kada bi svaki tim te provere pisao sam, provere bi se razlikovale od modula do modula, a neke bi bile nepotpune.

Zato slike prima jedan modul. On sliku proveri, sačuva pod imenom koje sam odredi i vrati njenu adresu. Feature modul čuva samo tu adresu, kao običan tekst, na isti način na koji čuva identifikator korisnika.

## Pravila

Modul prihvata sliku samo kada važi sledeće:

- zahtev šalje prijavljen korisnik, bilo koje uloge;
- fajl nije prazan i nije veći od 5 MB;
- fajl je JPEG, PNG ili WebP, a vrsta se utvrđuje iz sadržaja fajla, ne iz ekstenzije;
- slika nije veća od 4000 × 4000 piksela.

Sačuvana slika dobija ime koje modul sam izmisli, nikada ime koje je poslao korisnik. Sačuvana slika je javna: vidi je svako ko ima njenu adresu.

## Krajnje tačke

- `POST /api/media/images` prima jedan fajl u polju `file` (forma tipa `multipart/form-data`) i vraća `{ "address": "/api/media/images/<ime>" }`. Kada fajl ne prođe proveru, vraća odgovor sa statusnim kodom 400 i razlogom u polju `title`.
- `GET /api/media/images/<ime>` vraća sliku. Kada slika ne postoji, vraća odgovor sa statusnim kodom 404.

## Kako modul koristi slike

1. Klijentska komponenta pošalje sliku na `POST /api/media/images` i dobije adresu.
2. Forma modula tu adresu šalje svom serveru, u svojoj DTO strukturi, kao tekst (npr. svojstvo `Images` agregata `Blog`).
3. Stranica modula prikazuje sliku elementom `img` čiji je `src` ta adresa.

Modul nikada ne referencira projekat `Media` i ne poziva njegove klase. Arhitektonski test to proverava.

## Odstupanje od strukture modula

Kao i Identity, modul ima jedan projekat sa direktorijumima `Api`, `Core` i `Infrastructure`, umesto pet projekata. Nema bazu podataka: slike su datoteke na disku, u direktorijumu iz podešavanja `Media:StoragePath`. U razvojnom okruženju to je `backend/media-storage/`, koji se ne commit-uje.

- `Core` sadrži vrednosni objekat `ImageFile`, koji u konstruktoru proverava veličinu fajla i dimenzije slike, grupu slučajeva korišćenja `ImageUploading` i interfejse za čitanje zaglavlja slike i za skladište.
- `Infrastructure` sadrži lokalnog tehničkog stručnjaka `SkiaImageInspector`, koji kroz biblioteku SkiaSharp čita format i dimenzije iz zaglavlja fajla, i `DiskImageStorage`, koji sliku upisuje na disk pod imenom koje sam odredi.
- `Api` sadrži kontroler `ImageUploadingController`.
