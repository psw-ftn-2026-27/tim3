import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { CheckoutDto, EnterCouponDto } from '../api/payment-api-types';

export const CART_URL = '/api/payment/cart';

@Injectable({ providedIn: 'root' })
export class ShoppingCart {
  private readonly http = inject(HttpClient);

  async add(tourId: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${CART_URL}/items/${tourId}`, {}));
  }

  async remove(tourId: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${CART_URL}/items/${tourId}`));
  }

  async enterCoupon(dto: EnterCouponDto): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${CART_URL}/coupons`, dto));
  }

  async removeCoupon(couponId: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${CART_URL}/coupons/${couponId}`));
  }

  async checkout(dto: CheckoutDto): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${CART_URL}/checkout`, dto));
  }
}
