import { Injectable, signal } from '@angular/core';

const THEME_KEY = 'explorer.theme';

// Light or dark theme of the whole page. The dark colours are in _tokens.scss, under
// :root[data-theme='dark']; this service only switches that attribute on <html>.
// The first theme is chosen by a small script in index.html, before the app loads, so a
// dark page does not flash light: the saved choice, or else the system setting.
@Injectable({ providedIn: 'root' })
export class Theme {
  readonly dark = signal(document.documentElement.dataset['theme'] === 'dark');

  // The new theme spreads as a growing circle from the toggle button (see _base.scss).
  // Browsers without view transitions, and users who ask for less motion, switch at once.
  toggle(button: HTMLElement): void {
    const still = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    if (!('startViewTransition' in document) || still) {
      this.switch();
      return;
    }

    const box = button.getBoundingClientRect();
    const page = document.documentElement;
    page.style.setProperty('--reveal-x', `${box.left + box.width / 2}px`);
    page.style.setProperty('--reveal-y', `${box.top + box.height / 2}px`);
    document.startViewTransition(() => this.switch());
  }

  private switch(): void {
    this.dark.update((dark) => !dark);
    localStorage.setItem(THEME_KEY, this.dark() ? 'dark' : 'light');
    document.documentElement.dataset['theme'] = this.dark() ? 'dark' : 'light';
  }
}
