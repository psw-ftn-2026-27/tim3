# Vodič za stilske klase

Zajedničke klase iz `_components.scss`, `_layout.scss` i `_base.scss` daju svakoj stranici isti izgled u svetlom i tamnom režimu, bez sopstvenog stila. Za svaku klasu vodič kaže kada se koristi i daje kratak primer koji se kopira. Primeri su iz postojećih stranica, a fajl stranice je naveden uz primer.

Platformski član tima pregleda svaku novu ili izmenjenu stranicu u PR-u prema ovom vodiču. Odstupanje dobija komentar sa linkom na odeljak, na primer `frontend/src/styles/README.md#error-uz-polje`, a PR se spaja kada su sva odstupanja ispravljena.

Klase su u vlasništvu platformskog tima.

## Stanja stranice

Stranica koja čita resurs prikazuje tačno jedno stanje: učitavanje, grešku učitavanja, prazan spisak ili podatke. Svako stanje ima svoju klasu, pa izgleda isto u svakom modulu.

### `.loading`

Dok resurs stiže sa servera. Kratka siva rečenica stoji na mestu podataka, a za njom dolaze grane za grešku učitavanja, prazan spisak i podatke. Greška učitavanja i prazan spisak su `.empty-state`, opisan u sledećem odeljku (`tour-list.html`):

```html
@if (tours.isLoading()) {
  <p class="loading">Loading tours...</p>
} @else if (tours.error()) {
  <div class="empty-state" role="alert"><!-- vidi odeljak .empty-state --></div>
} @else if (visibleTours().length === 0) {
  <div class="empty-state"><!-- vidi odeljak .empty-state --></div>
} @else {
  <div class="grid">
    @for (tour of visibleTours(); track tour.id) {
      <app-tour-card [tour]="tour" />
    }
  </div>
}
```

### `.empty-state`

Kada stranica nema šta da prikaže: spisak ili tabela na nivou stranice nema nijednu stavku, ili resurs stranice ne može da se učita. Centrirana staklena kartica stoji umesto spiska, tabele ili podataka i ima tri dela: naslov `<h2>` koji kaže šta se desilo, jednu rečenicu `<p>` koja kaže zašto i šta korisnik može da uradi i red dugmadi `.empty-state-actions`. Akcija je `.button`: `<a>` sa `routerLink` ka stranici istog modula ili ka `/login`, ili `<button>` koji poziva metodu stranice. Glavna akcija dobija i `.button--primary`.

Pravilo: akcija odgovara uzroku. Kartica nudi samo akciju koja otklanja baš taj uzrok, a ako takve nema, nema ni dugmeta:

- greška učitavanja sa statusom 401 (korisnik nije prijavljen ili mu je sesija istekla, što se desi i dok klijent još misli da je prijavljen): jedna glavna akcija „Log in" ka `/login`, bez „Try again";
- status 403 (prijavljen je, ali nema ulogu, na primer kvizove vidi samo administrator): naslov kaže ko sme, a akcija vodi tamo gde korisnik može nešto da uradi u istom modulu (na primer `/games`), ili je nema; bez „Try again" i bez „Log in";
- status 404 na stranici jedne stavke (tura, blog, klub ne postoji): akcija vodi na spisak tih stavki;
- svaka druga greška (server ne radi, 500, mreža): „Could not load …", rečenica „Something went wrong on our side. Try again in a moment." i dugme „Try again" koje poziva `reload()` resursa;
- prazan spisak: akcija koja ga puni. Aktivni filteri daju „Clear filters", uz akciju za pravljenje kada je korisnik sme da koristi; prazan spisak koji korisnik sme da popuni daje akciju za pravljenje („Create tour"), pod istim `@if` uslovom pod kojim stranica tu akciju prikazuje i drugde; kada korisnik ne može ništa, kartica nema dugme, a rečenica kaže kada se stavke pojavljuju.

Status greške stranica čita iz resursa kroz `computed`, po jedan za svaki resurs (`tour-list.ts`):

```ts
protected readonly toursErrorStatus = computed(() => {
  const failure = this.tours.error();
  return failure instanceof HttpErrorResponse ? failure.status : null;
});
```

Šablon bira karticu kroz `@switch`, i navodi samo statuse koje endpoint zaista vraća. Kartica greške ima `role="alert"`, pa je čitač ekrana odmah pročita (`my-tours.html`):

```html
} @else if (tours.error()) {
  @switch (toursErrorStatus()) {
    @case (401) {
      <div class="empty-state" role="alert">
        <h2>Log in to see your tours</h2>
        <p>Your tours are shown to logged-in explorers, and your session may have expired.</p>
        <div class="empty-state-actions">
          <a class="button button--primary" routerLink="/login">Log in</a>
        </div>
      </div>
    }
    @case (403) {
      <div class="empty-state" role="alert">
        <h2>Only explorers can author tours</h2>
        <p>Explorers create and manage tours. You can still browse the published ones.</p>
        <div class="empty-state-actions">
          <a class="button button--primary" routerLink="/exploration">All tours</a>
        </div>
      </div>
    }
    @default {
      <div class="empty-state" role="alert">
        <h2>Could not load your tours</h2>
        <p>Something went wrong on our side. Try again in a moment.</p>
        <div class="empty-state-actions">
          <button class="button" type="button" (click)="tours.reload()">Try again</button>
        </div>
      </div>
    }
  }
}
```

Prazan spisak. Tabela ili mreža kartica se tada ne prikazuje, pa petlja `@for` nema blok `@empty`. Kada su filteri aktivni, spisak puni „Clear filters"; kada nema nijedne ture, puni ga nova tura (`tour-list.html`):

```html
} @else if (visibleTours().length === 0) {
  @if (hasFilters()) {
    <div class="empty-state">
      <h2>No tours match your filters</h2>
      <p>
        Try a different name or set Difficulty to All.
        @if (auth.isLoggedIn()) {
          You can also create a tour of your own.
        }
      </p>
      <div class="empty-state-actions">
        <button class="button" type="button" (click)="clearFilters()">Clear filters</button>
        @if (auth.isLoggedIn()) {
          <a class="button button--primary" routerLink="/exploration/create">Create tour</a>
        }
      </div>
    </div>
  } @else {
    <div class="empty-state">
      <h2>No tours published yet</h2>
      <p>
        @if (auth.isLoggedIn()) {
          Create the first tour and publish it for explorers.
        } @else {
          Tours appear here when their authors publish them.
        }
      </p>
      @if (auth.isLoggedIn()) {
        <div class="empty-state-actions">
          <a class="button button--primary" routerLink="/exploration/create">Create tour</a>
        </div>
      }
    </div>
  }
}
```

Unutar `.card` (odeljak stranice ili stranica koja je cela jedna kartica, kao `edit-blog.html` i `payment-home.html`) kartica je već staklo, pa `.empty-state` tu samo centrira svoj sadržaj, bez drugog okvira.

### `.empty`

Kada mali spisak unutar kartice ili forme nema nijednu stavku: vremena prevoza u kartici ture, slike u formi, članovi kluba, komentari ispod bloga. Spisak na nivou stranice umesto toga dobija `.empty-state`. `.empty` je kratka siva rečenica koja kaže i šta korisnik može da uradi, a stoji u bloku `@empty` petlje `@for` (`tour-card.html`):

```html
} @empty {
  <li class="empty">No transport times yet. Check back later.</li>
}
```

### `.error` uz polje

Greška validacije jednog polja forme. Stoji unutar `.field`, ispod kontrole, i nema `role`. Prikazuje se kao kratak crven red, a okvir polja je crven dok je poruka vidljiva (`create-tour.html`):

```html
<div class="field">
  <label for="name">Name</label>
  <input id="name" type="text" [formField]="form.name" />
  @if (form.name().touched()) {
    @for (message of form.name().errors(); track $index) {
      <p class="error">{{ message.message }}</p>
    }
  }
</div>
```

### `.error` kao poruka stranice

Kada komanda ne uspe (čuvanje forme, radnja u redu tabele). Uvek ima `role="alert"`, pa se prikazuje kao obojen blok sa oznakom „Error" iznad teksta. Stoji tamo gde je korisnik završio radnju: u njenoj formi, iznad dugmeta za slanje (`create-tour.html`):

```html
@if (error()) {
  <p class="error" role="alert">{{ error() }}</p>
}
```

ili u redu ispod reda tabele na kom je kliknuo (`my-tours.html`):

```html
@if (rowError()?.rowId === tour.id) {
  <tr>
    <td colspan="6"><p class="error" role="alert">{{ rowError()?.message }}</p></td>
  </tr>
}
```

Resurs koji ne može da se učita nije `.error`, nego `.empty-state` sa `role="alert"` (odeljak `.empty-state`).

### `.success`

Kada komanda uspe, a njen rezultat se ne vidi tamo gde je korisnik završio radnju, na primer kada se forma posle čuvanja zatvori. Ima `role="status"` i prikazuje se kao zelen blok sa oznakom „Success". Kada se rezultat vidi na mestu radnje (nov status u redu tabele), poruka o uspehu se ne prikazuje. Sledeća komanda briše poruku, pa komanda na početku postavlja signal poruke na `null` (`my-tours.html`):

```html
@if (success()) {
  <p class="success" role="status">{{ success() }}</p>
}
```

## Ostale zajedničke klase

### `.button` i `.badge`

Akcija je dugme, stanje je oznaka. Sve na šta korisnik klikne da nešto uradi ili ode na drugu stranicu je `.button`: `<button>` za komandu, `<a>` sa `routerLink` za odlazak na stranicu. Glavna akcija (slanje forme, „Publish", „Create tour") dobija i `.button--primary`. Stanje ili oznaka („Draft", „Published", tag) je `.badge`; nije klikabilna i nema hover, pa nikada nije akcija (`my-tours.html`):

```html
<td>
  <span class="badge">{{ tour.status }}</span>
</td>
<td>
  @if (tour.status === 'Draft') {
    <button class="button button--primary" type="button" [disabled]="pending()" (click)="selectForPublish(tour.id)">
      Publish
    </button>
  }
</td>
```

### `.card`

Okvir za jednu celinu: stavku spiska (tura, blog), formu ili odeljak stranice. Kartica koja je `<article>` ili `<a>` naginje se ka pokazivaču, a ostale (forma, odeljak) samo posvetle pod njim. Uz `.stack` sadržaj kartice dobija razmak (`create-tour.html`):

```html
<div class="card stack">
  <h1>Create tour</h1>

  <form class="stack" (submit)="submit($event)">
    <!-- polja, greška komande i dugme za slanje -->
  </form>
</div>
```

Stavka spiska je `<article class="card">` (`tour-card.html`, `blog-card.html`).

### `.field`

Svako polje forme: `<label>` i kontrola (`input`, `select` ili `textarea`) u jednom `<div class="field">`, sa greškama polja ispod kontrole (odeljak `.error` uz polje). Labela se vezuje za kontrolu kroz `for` i `id` (`create-tour.html`):

```html
<div class="field">
  <label for="difficulty">Difficulty</label>
  <select id="difficulty" [formField]="form.difficulty">
    <option value="Easy">Easy</option>
    <option value="Moderate">Moderate</option>
    <option value="Hard">Hard</option>
  </select>
</div>
```

### `.table` i `.table-wrapper`

Spisak sa više kolona koje korisnik poredi (moje ture, svi kvizovi). Tabela uvek stoji u `.table-wrapper`, pa na telefonu široka tabela skroluje u stranu unutar omotača, a ne cela stranica. Prva kolona je naziv stavke. Kada je spisak prazan, umesto tabele stoji `.empty-state`, pa tabela nema red za prazan spisak (`my-tours.html`):

```html
<div class="table-wrapper">
  <table class="table">
    <thead>
      <tr>
        <th scope="col">Name</th>
        <th scope="col">Status</th>
      </tr>
    </thead>
    <tbody>
      @for (tour of tours.value() ?? []; track tour.id) {
        <tr>
          <td>{{ tour.name }}</td>
          <td><span class="badge">{{ tour.status }}</span></td>
        </tr>
      }
    </tbody>
  </table>
</div>
```

### `.page-head` i `.page-actions`

Vrh stranice: naslov levo, akcije cele stranice desno, a na uskom ekranu akcije prelaze ispod naslova. Akcije koje važe za celu stranicu („All tours", „Create tour") stoje ovde, a ne razbacane po stranici (`my-tours.html`):

```html
<div class="page-head">
  <h1>My tours</h1>

  <div class="page-actions">
    <a class="button" routerLink="/exploration">All tours</a>
    <a class="button button--primary" routerLink="/exploration/create">Create tour</a>
  </div>
</div>
```

### `.stack`, `.grid` i `.container`

`.stack` daje razmak između elemenata poređanih jedan ispod drugog: na korenu stranice, u kartici i u formi. `.grid` slaže kartice u kolone koje se prilagođavaju širini ekrana. `.container` već postoji u okviru aplikacije (`app.html`), pa ga stranica ne dodaje (`tour-list.html`):

```html
<div class="stack">
  <div class="page-head">
    <h1>Tours</h1>
  </div>

  <div class="grid">
    @for (tour of visibleTours(); track tour.id) {
      <app-tour-card [tour]="tour" />
    }
  </div>
</div>
```

### Link u tekstu

Link unutar rečenice je podvučen, pa korisnik vidi da može da klikne. Klasa nije potrebna: dovoljno je da `<a>` stoji u `<p>`. Link koji stoji sam (navigacija, naslov kartice) i link kao dugme (`class="button"`) nisu podvučeni (`login.html`):

```html
<p>No account yet? <a routerLink="/register">Register</a></p>
```

## Pravila

- Stranica ne piše sopstveni stil za ono što zajednička klasa već pokriva: ne boji svoju grešku, ne pravi svoje dugme, oznaku ili karticu i ne zadaje razmak koji daje `.stack`. U `.scss` stranice ide samo ono što je samo njeno, na primer raspored filtera.
- Klasa koja treba za više modula, a ne postoji, dodaje se u `_components.scss` ili `_layout.scss` po pravilu promocije iz odeljka Styling u `frontend/AGENTS.md`. To je zajednički kod, pa se dodaje u dogovoru sa platformskim timom.
