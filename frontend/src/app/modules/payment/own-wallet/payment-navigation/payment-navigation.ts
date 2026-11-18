import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-payment-navigation',
  imports: [RouterLink],
  templateUrl: './payment-navigation.html',
  styleUrl: './payment-navigation.scss',
})
export class PaymentNavigation {
  readonly current = input.required<'wallet' | 'cart' | 'gift-cards' | 'coupons'>();
}
