import { Component, input, output } from '@angular/core';
import { RouterLink } from '@angular/router';

// The card a profile list shows when it cannot load. The four lists of the profile fail in
// the same way, so they share this card; the page passes the error status and the name of
// the list, and reloads its own resource when "Try again" is clicked.
@Component({
  imports: [RouterLink],
  selector: 'app-profile-load-error',
  styleUrl: './profile-load-error.scss',
  templateUrl: './profile-load-error.html',
})
export class ProfileLoadError {
  // The HTTP status of the failed request, or null when there is none (a network failure).
  readonly status = input.required<number | null>();
  // What the list holds, as it reads in a sentence: "followers", "sent requests".
  readonly what = input.required<string>();

  readonly retry = output();
}
