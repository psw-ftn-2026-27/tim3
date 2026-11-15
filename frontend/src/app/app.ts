import { Component, inject, signal } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { Auth } from './core/auth/auth';
import { Theme } from './core/theme/theme';

// Elements that light up under the pointer, and the cards among them that also tilt.
const GLASS = '.button, .card, .table tbody tr';
const TILTING = 'a.card, article.card';

@Component({
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  selector: 'app-root',
  styleUrl: './app.scss',
  templateUrl: './app.html',
  host: {
    '(document:pointermove)': 'followPointer($event)',
    '(document:pointerdown)': 'pressCard()',
    '(document:pointerup)': 'releaseCard()',
    '(document:pointerout)': 'leaveWindow($event)',
  },
})
export class App {
  protected readonly auth = inject(Auth);
  protected readonly theme = inject(Theme);

  // Position and width of the shared pill behind the nav links, in pixels.
  protected readonly indicatorLeft = signal(0);
  protected readonly indicatorWidth = signal(0);
  protected readonly indicatorShown = signal(false);
  // True when the pill moves from one link to another instead of appearing.
  protected readonly indicatorSliding = signal(false);

  // The glass element under the pointer, and whether the mouse button is held on it.
  private lit: HTMLElement | null = null;
  private pressed = false;
  private readonly still = window.matchMedia('(prefers-reduced-motion: reduce)').matches;

  protected moveIndicator(event: Event) {
    const link = event.target as HTMLElement;
    // A visible pill glides to the new link; a hidden one appears under it and grows from its centre.
    this.indicatorSliding.set(this.indicatorShown());
    this.indicatorLeft.set(link.offsetLeft);
    this.indicatorWidth.set(link.offsetWidth);
    this.indicatorShown.set(true);
  }

  protected hideIndicator() {
    this.indicatorSliding.set(false);
    this.indicatorShown.set(false);
  }

  protected hideIndicatorOnBlur(event: FocusEvent) {
    // Moving focus from one nav link to the next keeps the pill, so it can glide.
    const links = event.currentTarget as HTMLElement;
    if (!links.contains(event.relatedTarget as Node | null)) {
      this.hideIndicator();
    }
  }

  protected toggleTheme(event: Event) {
    this.theme.toggle(event.currentTarget as HTMLElement);
  }

  // The light follows the pointer: the element under it gets the pointer position
  // (--mx, --my) and --glow 1, which the global styles use for its light spot.
  protected followPointer(event: PointerEvent) {
    const element = (event.target as Element).closest<HTMLElement>(GLASS);
    if (element !== this.lit) {
      this.darken();
      this.lit = element;
    }
    if (!element) {
      return;
    }

    const box = element.getBoundingClientRect();
    const x = Math.min(1, Math.max(0, (event.clientX - box.left) / box.width));
    const y = Math.min(1, Math.max(0, (event.clientY - box.top) / box.height));
    element.style.setProperty('--mx', `${x * 100}%`);
    element.style.setProperty('--my', `${y * 100}%`);
    element.style.setProperty('--glow', '1');

    // A tilting card leans toward the pointer and lifts a little.
    if (element.matches(TILTING) && !this.still) {
      element.style.setProperty('--rx', `${(0.5 - y) * 9}deg`);
      element.style.setProperty('--ry', `${(x - 0.5) * 11}deg`);
      element.style.setProperty('--lift', '-6px');
      element.style.setProperty('--scale', this.pressed ? '0.99' : '1.015');
    }
  }

  // Holding the mouse button on a tilting card presses it down slightly.
  protected pressCard() {
    this.pressed = true;
    if (this.lit?.matches(TILTING) && !this.still) {
      this.lit.style.setProperty('--scale', '0.99');
    }
  }

  protected releaseCard() {
    this.pressed = false;
    if (this.lit?.matches(TILTING) && !this.still) {
      this.lit.style.setProperty('--scale', '1.015');
    }
  }

  // The pointer left the browser window, so nothing stays lit.
  protected leaveWindow(event: PointerEvent) {
    if (!event.relatedTarget) {
      this.darken();
      this.lit = null;
    }
  }

  // Removing the properties returns the element to its still look, with a transition.
  // The pointer position stays, so the light fades out where it was.
  private darken() {
    for (const property of ['--glow', '--rx', '--ry', '--lift', '--scale']) {
      this.lit?.style.removeProperty(property);
    }
  }
}
