import { effect, inject, Injectable, untracked } from '@angular/core';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';
import { Auth } from '../../core/auth/auth';
import { Consent } from '../../core/consent/consent';
import { AnalyticsEvents } from './analytics-events';

const MEASUREMENT_ID = environment.analyticsMeasurementId;

// Every ID on the server is a GUID; in a sent path each one is replaced with ':id'.
const GUID = /[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}/gi;

// The window properties Google's script reads: the command queue, and the switch that
// turns sending off for this measurement ID.
const google = window as unknown as Record<string, unknown> & { dataLayer?: unknown[] };

// Google's gtag function: it only queues its arguments, and gtag.js sends them once loaded.
// gtag.js expects the arguments object itself, not an array, so this is not an arrow function.
const gtag = function () {
  // eslint-disable-next-line prefer-rest-params
  google.dataLayer?.push(arguments);
} as (...args: unknown[]) => void;

// Sends usage data to Google Analytics, only while the user has accepted analytics and a
// measurement ID is configured. Google's script is loaded on the first accept, never before.
// It sends page views on its own, and a page records an event from the list in
// analytics-events.ts with record(), right after a successful command.
@Injectable({ providedIn: 'root' })
export class Analytics {
  private readonly router = inject(Router);
  private readonly auth = inject(Auth);
  private readonly consent = inject(Consent);

  private loaded = false;
  // The path of the last page view sent, so a change of only the query is not a new view.
  private lastPath: string | null = null;

  // Platform exception to the Angular subset: a navigation or an accept must reach Google,
  // which is outside the component tree. Runs after every successful navigation and every
  // change of the consent choice.
  private readonly pageViews = effect(() => {
    const url = this.router.lastSuccessfulNavigation()?.finalUrl;
    if (!this.consent.accepted()) {
      this.stop();
      return;
    }
    this.start();
    if (!url) {
      return;
    }
    // The path without ?query and #hash, which can hold what the user typed.
    const path = this.router.serializeUrl(url).split(/[?#]/)[0];
    if (path !== this.lastPath) {
      this.lastPath = path;
      // An ID in the path, such as the two users of a chat, never goes to Google.
      const sentPath = path.replace(GUID, ':id');
      const pageLocation = location.origin + sentPath;
      // Google's own hits use this address too, instead of the full one from the browser.
      gtag('set', { page_location: pageLocation });
      this.send('page_view', { page_path: sentPath, page_location: pageLocation });
    }
  });

  // Records an event from the list in analytics-events.ts. The compiler accepts a name only
  // with its own parameters. Never throws: without consent or a measurement ID nothing is
  // sent, and a blocked script only leaves the queue unread.
  record<K extends keyof AnalyticsEvents>(name: K, params: AnalyticsEvents[K]): void {
    this.send(name, params);
  }

  // The single place that sends to Google: checks consent and adds the visitor's role,
  // after the parameters, so no caller can replace it.
  // Never send an email, a name or an identifier.
  private send(name: string, params: Record<string, string | number>): void {
    if (!this.consent.accepted()) {
      return;
    }
    gtag('event', name, { ...params, role: untracked(() => this.role()) });
  }

  private role(): string {
    const user = this.auth.user();
    if (user === null) {
      return 'guest';
    }
    return user.roles.includes('administrator') ? 'administrator' : 'explorer';
  }

  private start(): void {
    google[`ga-disable-${MEASUREMENT_ID}`] = false;
    if (this.loaded) {
      return;
    }
    this.loaded = true;
    google.dataLayer = google.dataLayer ?? [];
    gtag('js', new Date());
    // Page views are sent by hand, so the full URL with its query never goes to Google.
    gtag('config', MEASUREMENT_ID, { send_page_view: false });

    // Nothing waits for the script, so an ad blocker that stops it changes nothing for the user.
    const script = document.createElement('script');
    script.async = true;
    script.src = `https://www.googletagmanager.com/gtag/js?id=${MEASUREMENT_ID}`;
    script.onerror = () => undefined;
    document.head.appendChild(script);
  }

  // After a revoke the next accept sends the current page again.
  private stop(): void {
    this.lastPath = null;
    if (this.loaded) {
      google[`ga-disable-${MEASUREMENT_ID}`] = true;
    }
  }
}
