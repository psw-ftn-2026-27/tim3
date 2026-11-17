import { computed, Injectable, signal } from '@angular/core';
import { environment } from '../../../environments/environment';

const CONSENT_KEY = 'explorer.analytics-consent';

export type ConsentChoice = 'accepted' | 'declined';

// The user's choice about analytics, remembered in the browser like the theme. The bar in
// the app shell asks for it; until the user chooses, analytics is off. Without a measurement
// ID in the environment there is no analytics, so the bar is never shown.
@Injectable({ providedIn: 'root' })
export class Consent {
  readonly configured = environment.analyticsMeasurementId !== '';

  // The stored choice, or null when the user has not chosen yet.
  readonly choice = signal<ConsentChoice | null>(readChoice());
  // True when the user opened the bar again with the "Cookie settings" link.
  private readonly reopened = signal(false);

  readonly barShown = computed(
    () => this.configured && (this.choice() === null || this.reopened()),
  );

  accept(): void {
    this.choose('accepted');
  }

  decline(): void {
    this.choose('declined');
  }

  reopen(): void {
    this.reopened.set(true);
  }

  private choose(choice: ConsentChoice): void {
    this.choice.set(choice);
    this.reopened.set(false);
    // A browser that blocks storage still gets the choice for this visit.
    try {
      localStorage.setItem(CONSENT_KEY, choice);
    } catch {
      // Nothing to do: the bar asks again on the next visit.
    }
  }
}

// A blocked storage, or a value this code did not write, counts as no choice.
function readChoice(): ConsentChoice | null {
  try {
    const stored = localStorage.getItem(CONSENT_KEY);
    return stored === 'accepted' || stored === 'declined' ? stored : null;
  } catch {
    return null;
  }
}
