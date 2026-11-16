import { Routes } from '@angular/router';
import { PaymentHome } from './own-wallet/payment-home/payment-home';
import { CartPage } from './shopping-cart/cart-page/cart-page';

export const paymentRoutes: Routes = [
  { path: 'cart', component: CartPage },
  { path: '', component: PaymentHome },
];
