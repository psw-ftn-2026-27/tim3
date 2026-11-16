import { httpResource } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { serverMessage } from '../../../../shared/util/server-message';
import { ProductForCartDto, ShoppingCartDto } from '../../api/payment-api-types';
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
  protected readonly selectedProduct = httpResource<ProductForCartDto>(() => {
    const tourId = this.tourId();
    return this.canViewCart() && tourId
      ? `${CART_URL}/products/${encodeURIComponent(tourId)}`
      : undefined;
  });
  protected readonly pending = signal(false);
  protected readonly addError = signal<{ tourId: string; message: string } | null>(null);
  protected readonly removeError = signal<string | null>(null);

  protected async add(tourId: string): Promise<void> {
    if (this.pending() || this.cart.isLoading()) {
      return;
    }
    this.pending.set(true);
    this.addError.set(null);
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
    try {
      await this.shoppingCart.remove(tourId);
      this.cart.reload();
    } catch (error) {
      this.removeError.set(serverMessage(error, 'Uklanjanje ture nije uspelo.'));
    } finally {
      this.pending.set(false);
    }
  }
}
