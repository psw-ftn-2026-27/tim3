# Beleženje događaja

Servis `Analytics` (`analytics.ts`) je deljeni servis platforme koji šalje podatke o korišćenju u Google Analytics, samo kada je korisnik prihvatio analitiku. Posete stranama beleži sam. Ti preko njega beležiš događaje iz svog modula, na primer da je tura objavljena, a servis uz svaki događaj sam dodaje ulogu korisnika (`guest`, `explorer` ili `administrator`). Modul nikada ne poziva Google Analytics direktno, ni `gtag`, ni `dataLayer`.

## Kako se beleži događaj

Događaj beleži stranica, odmah posle uspešne komande. Stranica injektuje `Analytics` i poziva `record` posle `await` komande, unutar `try`, pre ponovnog učitavanja podataka:

```ts
import { Analytics } from '../../../../shared/analytics/analytics';

// u komponenti stranice
private readonly analytics = inject(Analytics);

protected async publish(event: Event): Promise<void> {
  // ... provere i pending, kao i do sada
  try {
    await this.tourAuthoring.publish(tourId, { isFree, amount });
    this.analytics.record('tour_published', { tour_id: tourId });
    this.tours.reload();
    this.publishTourId.set(null);
    this.success.set('Tura je objavljena.');
  } catch (failure) {
    this.formError.set(serverMessage(failure, 'Could not publish the tour.'));
  } finally {
    this.pending.set(false);
  }
}
```

Kada stranica komande izvršava kroz pomoćnu metodu, kao `run()` u Games-u, poziv ide u lambdu komande, posle `await`:

```ts
protected async publish(id: string): Promise<void> {
  await this.run(
    id,
    async () => {
      await this.socialChallengeAuthoring.publish(id);
      this.analytics.record('challenge_published', { challenge_id: id });
    },
    'Izazov je objavljen.',
    'Izazov ne može da se objavi.',
  );
}
```

- Kada komanda ne uspe, ništa se ne beleži: `record` stoji posle `await`, pa se do njega ne stiže.
- Događaj ne beleži servis grupe ni prikazna komponenta, samo stranica.
- Beleže se samo radnje koje nešto menjaju. Otvaranje strane se već beleži kao poseta, pa za to ne pravi događaj.

## Spisak imena

Jedina dozvoljena imena i njihovi parametri su u `analytics-events.ts`. Prevodilac prihvata samo ime sa spiska i samo parametre tog imena, pa pogrešno ime ili parametar ne prolazi `npm run build`.

Parametre piši kao objekat direktno u pozivu, kao u primerima gore. Prevodilac višak polja hvata samo na objektu napisanom u samom pozivu; objekat koji si pre toga sačuvao u promenljivoj može da prođe sa poljem koje ne pripada imenu.

## Dodavanje novog imena

Kada radnje nema na spisku, dodaj je u svom PR-u:

1. Dopiši ime i parametre u `analytics-events.ts`, i ništa drugo u `shared/`. To je jedini fajl u `shared/` koji timovi modula smeju da menjaju.
2. Ime ima oblik `<objekat>_<radnja u prošlom vremenu>`, malim slovima sa donjom crtom: `tour_published`, `gift_card_purchased`. Parametri se takođe pišu malim slovima sa donjom crtom (`tour_id`).
3. Vrednost parametra je tekst ili broj.
4. Platformski tim je obavezan recenzent te izmene. Posle spajanja platforma svaki nov parametar registruje u Google Analytics-u kao prilagođenu dimenziju događaja, da bi se video u izveštajima.

## Šta se nikad ne šalje

- Identifikator korisnika: `userId`, `recipientId`, id autora i slično. Id sadržaja (ture, kviza, bloga) je dozvoljen, jer ne otkriva ko je korisnik.
- Slobodan tekst koji je korisnik ukucao: naslov, poruka, tekst pretrage.
- Ime, email, ime za prikaz.

## Kada se ništa ne šalje

Bez saglasnosti korisnika ili bez ID-ja za merenje (`analyticsMeasurementId` je prazan) događaj se ne šalje. Ni tada, ni kada blokator reklama zaustavi Google-ovu skriptu, poziv `record` ne baca grešku, pa uspešna komanda nikad ne izgleda kao neuspela.

## Lokalna provera

1. U `src/environments/environment.development.ts` upiši bilo koji ID oblika `G-...` u `analyticsMeasurementId`. Taj ID nikada ne commit-uj.
2. Ponovo pokreni `npm start`.
3. U traci na dnu klikni „Accept".
4. Otvori DevTools, karticu Network, i filtriraj po `collect`.
5. Izvrši komandu. U Payload-u zahteva vidiš `en` (ime događaja), `ep.<parametar>` za svaki tekstualni parametar (za broj `epn.<parametar>`) i `ep.role` (ulogu).
6. Pre commit-a vrati prazan ID.
