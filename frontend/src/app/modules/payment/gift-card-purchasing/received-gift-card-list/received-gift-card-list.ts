import { Component, input } from '@angular/core';
import { GiftCardStatus, ReceivedGiftCardDto } from '../../api/payment-api-types';

@Component({
  selector: 'app-received-gift-card-list',
  templateUrl: './received-gift-card-list.html',
  styleUrl: './received-gift-card-list.scss',
})
export class ReceivedGiftCardList {
  readonly giftCards = input.required<ReceivedGiftCardDto[]>();

  protected statusLabel(status: GiftCardStatus): string {
    switch (status) {
      case 'Valid':
        return 'Važeća';
      case 'Expired':
        return 'Istekla';
      case 'Redeemed':
        return 'Iskorišćena';
    }
  }

  protected date(iso: string): string {
    return new Date(iso).toLocaleDateString('sr-Latn-RS');
  }
}
