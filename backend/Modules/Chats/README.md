# Modul Chats

Modul Chats je platformski modul zadužen za razgovor u četu: uz turu, uz izazov i između dva korisnika. Sastoji se od dva projekta. Projekat `Chats` sadrži sam modul, a projekat `Chats.Tests` njegove testove. Studenti ovaj modul koriste kroz njegove krajnje tačke, ali ga ne menjaju i ne referenciraju iz svojih modula.

## Problem koji rešava

Razgovor treba na više mesta: uz turu, uz izazov i između dva korisnika, a kasnije i u klubu. Kada bi svaki tim pisao svoj čet, isto pravilo o tome ko sme da čita i piše bi se ponavljalo na više mesta i razlikovalo od modula do modula.

Zato poruke čuva jedan modul. Čet ne poznaje turu, izazov ni korisnika kao entitete drugih modula: zna samo čemu pripada, kao vrstu i identifikator. Modul vlasnik stranice samo vodi korisnika na stranicu četa.

## Pravila

- Čet pripada tačno jednoj turi, jednom izazovu ili jednom paru korisnika. Poruke jednog četa se ne vide u drugom. Par mora imati dva različita korisnika, a par (A, B) je isti čet kao par (B, A).
- U četu ture i u četu izazova čita i piše svaki prijavljen korisnik, bilo koje uloge. U razgovoru dva korisnika čitaju i pišu samo njih dvoje. Neprijavljen korisnik ne sme ni da čita ni da piše.
- Poruka ne sme biti prazna, samo od razmaka, niti duža od 1000 znakova.
- Poruke se vraćaju u stranicama, od najnovije ka najstarijoj, pa je prva stranica poslednjih nekoliko poruka. Klijent ih na ekranu ređa redom kojim su poslate.
- Čet nastaje pri prvoj poruci. Upit nikada ne pravi čet: za čet koji još ne postoji vraća praznu listu.
- Poruke se ne menjaju. Nove poruke drugih korisnik vidi kada ponovo učita čet; poruke ne stižu u realnom vremenu.
- U četu ture privilegovan je autor ture, koga modul saznaje od modula Exploration kroz `Exploration.Contracts`. U četu izazova privilegovan je svaki administrator. U razgovoru dva korisnika niko nije privilegovan. Čet može imati nijednog, jednog ili više privilegovanih korisnika.
- Samo privilegovani korisnik briše tuđe poruke, utišava i ukida utišavanje. Privilegovani korisnik ne može biti utišan, niti se njegove poruke mogu brisati. Obrisana poruka nestaje bez traga.
- Utišan korisnik čita čet, ali u njemu ne može da piše. Utišavanje važi samo u četu u kome je izrečeno i traje dok ga privilegovani ne ukine. Ponovno utišavanje ili ukidanje ništa ne menja.
- Poznato ograničenje: u četu izazova modul zna samo ulogu onoga ko šalje zahtev, pa ne može da prepozna da je meta moderacije administrator.

## Krajnje tačke

Mesto četa je deo adrese: `tours/<id ture>`, `challenges/<id izazova>` ili `users/<id prvog korisnika>/<id drugog korisnika>`. Redosled dva korisnika u adresi nije bitan.

- `GET /api/chat/tours/<id>/messages?page=1&pageSize=20` (isto za `challenges/<id>` i `users/<id>/<id>`) vraća `{ "messages": { "items": [{ "id": ..., "senderId": ..., "senderName": "...", "text": "...", "sentAt": "...", "senderIsPrivileged": ... }], "totalCount": ... }, "viewerIsPrivileged": ..., "viewerIsMuted": ..., "mutedUsers": [{ "userId": ..., "displayName": "..." }] }`, sa porukama od najnovije do najstarije. Spisak `mutedUsers` dobija samo privilegovani korisnik; za ostale je prazan. Kada korisnik nije u paru čiji razgovor traži, vraća odgovor sa statusnim kodom 400.
- `POST /api/chat/tours/<id>/messages` (isto za `challenges/<id>` i `users/<id>/<id>`) sa telom `{ "text": "..." }` dodaje poruku i vraća odgovor sa statusnim kodom 204. Kada je poruka prazna ili predugačka, kada korisnik nije u paru, kada su oba korisnika para ista ili kada je korisnik utišan, vraća odgovor sa statusnim kodom 400 i razlogom u polju `title`.
- `DELETE /api/chat/tours/<id>/messages/<id poruke>` (isto za `challenges/<id>` i `users/<id>/<id>`) briše poruku i vraća odgovor sa statusnim kodom 204. Kada čet ne postoji ili poruka nije u tom četu, vraća 404; kada korisnik nije privilegovan ili je pošiljalac poruke privilegovan, vraća 400.
- `POST /api/chat/tours/<id>/muted-users/<id korisnika>` (isto za `challenges/<id>` i `users/<id>/<id>`) utišava korisnika, a `DELETE` na istu adresu ukida utišavanje; obe vraćaju 204. Kada čet ne postoji, vraćaju 404; kada korisnik nije privilegovan ili (pri utišavanju) je meta privilegovana, vraćaju 400.

Sve krajnje tačke traže prijavljenog korisnika; neprijavljen dobija odgovor sa statusnim kodom 401. Imena pošiljalaca i utišanih modul dobija od modula Identity kroz `Identity.Contracts`.

## Kako modul koriste drugi

1. Stranica ture, izazova ili pretrage korisnika ima link ka stranici četa, sa vrstom i identifikatorom u adresi.
2. Stranica četa čita poruke sa `GET` krajnje tačke i šalje nove na `POST` krajnju tačku.
3. Modul vlasnik stranice ne čuva ništa o četu.

Modul nikada ne referencira projekat `Chats` i ne poziva njegove klase. Arhitektonski test to proverava. Chats sam pita modul Exploration za autora ture kroz `IExplorationApi` iz `Exploration.Contracts`.

## Odstupanje od strukture modula

Kao i Identity, modul ima jedan projekat sa direktorijumima `Api`, `Core` i `Infrastructure`, umesto pet projekata. Poruke se čuvaju u sopstvenoj šemi `chats`.

- `Core` sadrži agregat `Chat` sa vrednosnim objektom `Subject` (čemu čet pripada; pravi se jednom od metoda `ForTour`, `ForChallenge` i `ForPair`, pa tura sa dva identifikatora ili par sa jednim ne mogu ni da se napišu; `ForPair` odbija par istog korisnika i uređuje par tako da redosled nije bitan, a metoda `Admits` kaže ko sme da čita i piše), spiskom utišanih `MutedUserIds` i metodama `Write` (pravi poruku posle provere `Subject.Admits` i provere da pošiljalac nije utišan), `Mute` i `Unmute`. Vrednosni objekat `Privileges` je skup privilegovanih korisnika jednog četa; njegova metoda `EnsureCanModerate` odbija moderaciju kada onaj ko moderira nije privilegovan ili kada je meta privilegovana, a koriste je utišavanje i brisanje. Čet ne zna ko je privilegovan: `Privileges` sastavlja klasa `ChatPrivilegesLookup` (autor ture kroz `IExplorationApi`, administrator po ulozi iz zahteva, u paru niko). Sadrži i agregat `Message`, čiji konstruktor proverava tekst, interfejse `IChatRepository`, `IChatReadRepository`, `IMessageRepository`, `IMessageReadRepository` i `IUnitOfWork`, grupu slučajeva korišćenja `ChatMessaging` (klase `ChatMessagingService` i `ChatMessagingQueries` i DTO strukture `SendMessageDto`, `MessageRowDto`, `MessageDto`, `MutedUserDto` i `ChatMessagesDto`) i grupu `ChatModeration` (klasa `ChatModerationService` sa metodama `DeleteMessageAsync`, `MuteUserAsync` i `UnmuteUserAsync`; brisanje uklanja ceo agregat `Message`).
- `Infrastructure/Persistence/` sadrži `ChatsDbContext` na šemi `chats`, EF konfiguraciju i repozitorijume oba agregata, migracije i `ChatsModuleInitializer`, koji pri pokretanju primenjuje migracije. Jedinstveni indeks nad vrstom i identifikatorima predmeta čuva pravilo da jednom predmetu pripada najviše jedan čet. Utišani korisnici se čuvaju u koloni `MutedUserIds` tabele `Chats`. Migracije se dodaju komandom `dotnet ef migrations add <Ime> --project Modules/Chats/Chats --startup-project Host.Api --context ChatsDbContext --output-dir Infrastructure/Persistence/Migrations`.
- `Api` sadrži kontrolere `ChatMessagingController` i `ChatModerationController`, koji od adrese prave `Subject`, uz to prosleđuju da li je pozivalac administrator i pozivaju servis ili upit.

Čet kluba se kasnije dodaje kao nova vrsta u `Subject`, sa svojom metodom `ForClub`, sa svojim pravilom u `Admits`, sa svojim izvorom privilegovanih u `ChatPrivilegesLookup` (vlasnik i moderatori kluba) i svojim rutama u kontrolerima, bez izmene agregata `Message`, repozitorijuma i upita.
