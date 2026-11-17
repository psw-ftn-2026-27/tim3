import { Component, input, output, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ClubDto } from '../../api/social-api-types';

@Component({
  imports: [RouterLink],
  selector: 'app-club-card',
  styleUrl: './club-card.scss',
  templateUrl: './club-card.html',
})
export class ClubCard {
  readonly club = input.required<ClubDto>();
  readonly canManage = input(false);
  readonly pending = input(false);
  // The message of this club's failed delete, shown under its buttons.
  readonly error = input<string | null>(null);

  readonly editClub = output<ClubDto>();
  readonly deleteClub = output<string>();

  protected readonly confirming = signal(false);

  protected confirmDelete(): void {
    this.confirming.set(false);
    this.deleteClub.emit(this.club().id);
  }
}
