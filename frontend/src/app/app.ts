import { Component, inject, signal } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { Auth } from './core/auth/auth';

@Component({
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  selector: 'app-root',
  styleUrl: './app.scss',
  templateUrl: './app.html',
})
export class App {
  protected readonly auth = inject(Auth);

  // Position and width of the shared line under the nav links, in pixels.
  protected readonly indicatorLeft = signal(0);
  protected readonly indicatorWidth = signal(0);
  // True when the line moves from one link to another instead of appearing.
  protected readonly indicatorSliding = signal(false);

  protected moveIndicator(event: Event) {
    const link = event.target as HTMLElement;
    // A visible line slides to the new link; a hidden one jumps under it and draws left to right.
    this.indicatorSliding.set(this.indicatorWidth() > 0);
    this.indicatorLeft.set(link.offsetLeft);
    this.indicatorWidth.set(link.offsetWidth);
  }

  protected hideIndicator() {
    this.indicatorSliding.set(false);
    this.indicatorWidth.set(0);
  }

  protected hideIndicatorOnBlur(event: FocusEvent) {
    // Moving focus from one nav link to the next keeps the line, so it can slide.
    const links = event.currentTarget as HTMLElement;
    if (!links.contains(event.relatedTarget as Node | null)) {
      this.hideIndicator();
    }
  }
}
