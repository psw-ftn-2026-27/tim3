import { Component, input, output } from '@angular/core';
import { RecipientDto } from '../../api/payment-api-types';

@Component({
  selector: 'app-recipient-list',
  templateUrl: './recipient-list.html',
  styleUrl: './recipient-list.scss',
})
export class RecipientList {
  readonly recipients = input.required<RecipientDto[]>();
  readonly totalCount = input.required<number>();
  readonly selectedId = input<string | null>(null);
  readonly disabled = input(false);

  readonly choose = output<RecipientDto>();
}
