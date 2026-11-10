import { Component, input, output, signal } from '@angular/core';
import { ClubDto } from '../../api/social-api-types';

@Component({
  selector: 'app-club-card',
  styleUrl: './club-card.scss',
  templateUrl: './club-card.html',
})
export class ClubCard {
  readonly club = input.required<ClubDto>();
  readonly canManage = input(false);
  readonly pending = input(false);

  readonly editClub = output<ClubDto>();
  readonly deleteClub = output<string>();

  protected readonly confirming = signal(false);

  protected confirmDelete(): void {
    this.confirming.set(false);
    this.deleteClub.emit(this.club().id);
  }
}
