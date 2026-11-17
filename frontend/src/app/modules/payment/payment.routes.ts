import { Routes } from '@angular/router';
import { GiftCards } from './gift-card-purchasing/gift-cards/gift-cards';
import { PaymentHome } from './own-wallet/payment-home/payment-home';

export const paymentRoutes: Routes = [
  { path: '', component: PaymentHome },
  { path: 'gift-cards', component: GiftCards },
];
