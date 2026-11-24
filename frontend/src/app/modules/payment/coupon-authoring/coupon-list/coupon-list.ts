import { Component, input } from '@angular/core';
import { CouponDto } from '../../api/payment-api-types';

@Component({
  selector: 'app-coupon-list',
  templateUrl: './coupon-list.html',
  styleUrl: './coupon-list.scss',
})
export class CouponList {
  readonly coupons = input.required<CouponDto[]>();

  protected date(isoDate: string): string {
    // ValidUntil is a calendar day without a time, so it is shown in UTC to keep the same day.
    return new Date(isoDate).toLocaleDateString('sr-Latn-RS', { timeZone: 'UTC' });
  }
}
