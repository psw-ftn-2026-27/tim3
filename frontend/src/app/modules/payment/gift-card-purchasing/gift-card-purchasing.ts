import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { BuyGiftCardDto } from '../api/payment-api-types';

const BASE_URL = '/api/payment/gift-cards';

@Injectable({ providedIn: 'root' })
export class GiftCardPurchasing {
  private readonly http = inject(HttpClient);

  async buy(dto: BuyGiftCardDto): Promise<void> {
    await firstValueFrom(this.http.post<void>(BASE_URL, dto));
  }
}
