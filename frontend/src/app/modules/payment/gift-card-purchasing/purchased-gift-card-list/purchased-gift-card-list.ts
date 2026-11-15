import { Component, input } from '@angular/core';
import { GiftCardStatus, PurchasedGiftCardDto } from '../../api/payment-api-types';

@Component({
  selector: 'app-purchased-gift-card-list',
  templateUrl: './purchased-gift-card-list.html',
  styleUrl: './purchased-gift-card-list.scss',
})
export class PurchasedGiftCardList {
  readonly giftCards = input.required<PurchasedGiftCardDto[]>();

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
