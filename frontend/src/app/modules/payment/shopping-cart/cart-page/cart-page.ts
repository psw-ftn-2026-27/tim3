import { httpResource } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { serverMessage } from '../../../../shared/util/server-message';
import { ProductForCartDto, ShoppingCartDto, WalletDto } from '../../api/payment-api-types';
import { CART_URL, ShoppingCart } from '../shopping-cart';

@Component({
  selector: 'app-cart-page',
  imports: [RouterLink],
  templateUrl: './cart-page.html',
})
export class CartPage {
  private readonly shoppingCart = inject(ShoppingCart);
  private readonly auth = inject(Auth);

  readonly tourId = input<string>();

  protected readonly canViewCart = computed(
    () => this.auth.user()?.roles.includes('explorer') ?? false,
  );
  protected readonly cart = httpResource<ShoppingCartDto>(() =>
    this.canViewCart() ? CART_URL : undefined,
  );
  protected readonly wallet = httpResource<WalletDto>(() =>
    this.canViewCart() ? '/api/payment/wallet/me?pageSize=1' : undefined,
  );
  protected readonly selectedProduct = httpResource<ProductForCartDto>(() => {
    const tourId = this.tourId();
    return this.canViewCart() && tourId
      ? `${CART_URL}/products/${encodeURIComponent(tourId)}`
      : undefined;
  });
  protected readonly pending = signal(false);
  protected readonly addError = signal<{ tourId: string; message: string } | null>(null);
  protected readonly removeError = signal<string | null>(null);
  protected readonly checkoutError = signal<string | null>(null);
  protected readonly checkoutConfirmed = signal(false);

  protected async add(tourId: string): Promise<void> {
    if (this.pending() || this.cart.isLoading()) {
      return;
    }
    this.pending.set(true);
    this.addError.set(null);
    this.checkoutError.set(null);
    this.checkoutConfirmed.set(false);
    try {
      await this.shoppingCart.add(tourId);
      this.cart.reload();
    } catch (error) {
      this.addError.set({ tourId, message: serverMessage(error, 'Dodavanje ture nije uspelo.') });
    } finally {
      this.pending.set(false);
    }
  }

  protected async remove(tourId: string): Promise<void> {
    if (this.pending() || this.cart.isLoading()) {
      return;
    }
    this.pending.set(true);
    this.removeError.set(null);
    this.checkoutError.set(null);
    this.checkoutConfirmed.set(false);
    try {
      await this.shoppingCart.remove(tourId);
      this.cart.reload();
    } catch (error) {
      this.removeError.set(serverMessage(error, 'Uklanjanje ture nije uspelo.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async checkout(): Promise<void> {
    if (
      this.pending() ||
      this.cart.isLoading() ||
      !this.cart.hasValue() ||
      this.cart.value().items.length === 0
    ) {
      return;
    }
    this.pending.set(true);
    this.checkoutError.set(null);
    this.checkoutConfirmed.set(false);
    try {
      await this.shoppingCart.checkout();
      this.addError.set(null);
      this.removeError.set(null);
      this.checkoutConfirmed.set(true);
      this.cart.reload();
      this.wallet.reload();
    } catch (error) {
      this.checkoutError.set(serverMessage(error, 'Kupovina nije uspela.'));
    } finally {
      this.pending.set(false);
    }
  }
}
