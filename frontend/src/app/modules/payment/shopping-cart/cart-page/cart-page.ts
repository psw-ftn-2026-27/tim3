import { HttpErrorResponse, httpResource } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { FormField, form, required } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { Analytics } from '../../../../shared/analytics/analytics';
import { serverMessage } from '../../../../shared/util/server-message';
import {
  CouponNotAppliedReason,
  GiftCardForCartDto,
  GiftCardNotAppliedReason,
  ProductForCartDto,
  ShoppingCartDto,
  WalletDto,
} from '../../api/payment-api-types';
import { PaymentNavigation } from '../../own-wallet/payment-navigation/payment-navigation';
import { CART_URL, ShoppingCart } from '../shopping-cart';

@Component({
  selector: 'app-cart-page',
  imports: [FormField, RouterLink, PaymentNavigation],
  templateUrl: './cart-page.html',
  styleUrl: './cart-page.scss',
})
export class CartPage {
  private readonly shoppingCart = inject(ShoppingCart);
  private readonly analytics = inject(Analytics);
  protected readonly auth = inject(Auth);

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
  protected readonly giftCards = httpResource<GiftCardForCartDto[]>(() =>
    this.canViewCart() ? `${CART_URL}/gift-cards` : undefined,
  );
  protected readonly selectedProduct = httpResource<ProductForCartDto>(() => {
    const tourId = this.tourId();
    return this.canViewCart() && tourId
      ? `${CART_URL}/products/${encodeURIComponent(tourId)}`
      : undefined;
  });
  protected readonly cartErrorStatus = computed(() => {
    const error = this.cart.error();
    return error instanceof HttpErrorResponse ? error.status : null;
  });
  protected readonly productErrorStatus = computed(() => {
    const error = this.selectedProduct.error();
    return error instanceof HttpErrorResponse ? error.status : null;
  });
  protected readonly walletErrorStatus = computed(() => {
    const error = this.wallet.error();
    return error instanceof HttpErrorResponse ? error.status : null;
  });
  protected readonly pending = signal(false);
  protected readonly adding = signal(false);
  protected readonly addedTourId = signal<string | null>(null);
  protected readonly removingTourId = signal<string | null>(null);
  protected readonly cartNotice = signal<string | null>(null);
  protected readonly selectedTourInCart = computed(
    () =>
      this.cart.hasValue() && this.cart.value().items.some((item) => item.tourId === this.tourId()),
  );
  protected readonly awaitingSelectedTour = computed(
    () =>
      this.selectedProduct.hasValue() &&
      !this.selectedTourInCart() &&
      this.addedTourId() !== this.selectedProduct.value().tourId,
  );
  protected readonly purchasing = signal(false);
  protected readonly missingEc = computed(() => {
    if (
      this.cart.isLoading() ||
      this.cart.error() ||
      !this.cart.hasValue() ||
      this.wallet.isLoading() ||
      this.wallet.error() ||
      !this.wallet.hasValue()
    ) {
      return 0;
    }
    return Math.max(0, this.cart.value().totalFromWalletInEc - this.wallet.value().balance);
  });
  protected readonly checkoutBlocked = computed(
    () =>
      this.pending() ||
      this.cart.isLoading() ||
      !!this.cart.error() ||
      !this.cart.hasValue() ||
      this.cart.value().items.length === 0 ||
      this.wallet.isLoading() ||
      !!this.wallet.error() ||
      !this.wallet.hasValue() ||
      this.missingEc() > 0,
  );
  protected readonly addError = signal<{ tourId: string; message: string } | null>(null);
  protected readonly removeError = signal<{ tourId: string; message: string } | null>(null);
  protected readonly checkoutError = signal<string | null>(null);
  protected readonly checkoutConfirmed = signal(false);
  protected readonly applyingCoupon = signal(false);
  protected readonly removingCouponId = signal<string | null>(null);
  protected readonly couponError = signal<string | null>(null);
  protected readonly removeCouponError = signal<{ couponId: string; message: string } | null>(null);
  protected readonly applyingGiftCard = signal(false);
  protected readonly removingGiftCard = signal(false);
  protected readonly giftCardError = signal<string | null>(null);
  protected readonly removeGiftCardError = signal<string | null>(null);

  protected readonly couponForm = form(signal({ code: '' }), (path) => {
    required(path.code, { message: 'Unesi kod kupona.' });
  });

  protected readonly giftCardForm = form(signal({ giftCardId: '' }), (path) => {
    required(path.giftCardId, { message: 'Izaberi poklon karticu.' });
  });

  protected async add(tourId: string, name: string): Promise<void> {
    if (this.pending() || this.cart.isLoading() || !this.cart.hasValue()) {
      return;
    }
    this.pending.set(true);
    this.adding.set(true);
    this.cartNotice.set(null);
    this.addError.set(null);
    this.checkoutError.set(null);
    this.checkoutConfirmed.set(false);
    try {
      await this.shoppingCart.add(tourId);
      this.focusCartTitle();
      this.addedTourId.set(tourId);
      this.cartNotice.set(`Tura „${name}” je dodata u korpu.`);
      this.cart.reload();
    } catch (error) {
      this.addError.set({ tourId, message: serverMessage(error, 'Dodavanje ture nije uspelo.') });
    } finally {
      this.adding.set(false);
      this.pending.set(false);
    }
  }

  protected async remove(tourId: string, name: string): Promise<void> {
    if (this.pending() || this.cart.isLoading() || !this.cart.hasValue()) {
      return;
    }
    this.pending.set(true);
    this.removingTourId.set(tourId);
    this.cartNotice.set(null);
    this.removeError.set(null);
    this.checkoutError.set(null);
    this.checkoutConfirmed.set(false);
    try {
      await this.shoppingCart.remove(tourId);
      this.focusCartTitle();
      if (this.addedTourId() === tourId) this.addedTourId.set(null);
      this.cartNotice.set(`Tura „${name}” je uklonjena iz korpe.`);
      this.cart.reload();
    } catch (error) {
      this.removeError.set({
        tourId,
        message: serverMessage(error, 'Uklanjanje ture nije uspelo.'),
      });
    } finally {
      this.removingTourId.set(null);
      this.pending.set(false);
    }
  }

  protected async enterCoupon(event: Event): Promise<void> {
    event.preventDefault();
    if (
      this.pending() ||
      this.cart.isLoading() ||
      !this.cart.hasValue() ||
      !this.couponForm().valid()
    ) {
      return;
    }
    this.pending.set(true);
    this.applyingCoupon.set(true);
    this.cartNotice.set(null);
    this.couponError.set(null);
    this.removeCouponError.set(null);
    this.checkoutError.set(null);
    try {
      await this.shoppingCart.enterCoupon({ code: this.couponForm.code().value() });
      this.couponForm().reset({ code: '' });
      this.cart.reload();
    } catch (error) {
      this.couponError.set(serverMessage(error, 'Kupon nije mogao da se primeni.'));
    } finally {
      this.applyingCoupon.set(false);
      this.pending.set(false);
    }
  }

  protected async removeCoupon(couponId: string): Promise<void> {
    if (this.pending() || this.cart.isLoading() || !this.cart.hasValue()) {
      return;
    }
    this.pending.set(true);
    this.removingCouponId.set(couponId);
    this.cartNotice.set(null);
    this.couponError.set(null);
    this.removeCouponError.set(null);
    this.checkoutError.set(null);
    try {
      await this.shoppingCart.removeCoupon(couponId);
      this.cart.reload();
    } catch (error) {
      this.removeCouponError.set({
        couponId,
        message: serverMessage(error, 'Uklanjanje kupona nije uspelo.'),
      });
    } finally {
      this.removingCouponId.set(null);
      this.pending.set(false);
    }
  }

  protected notAppliedLabel(reason: CouponNotAppliedReason): string {
    switch (reason) {
      case 'Expired':
        return 'kupon je istekao';
      case 'AlreadyUsed':
        return 'kupon je već iskorišćen';
      case 'NoMatchingItem':
        return 'u korpi nema proizvoda na koji se odnosi';
      case 'BetterCouponApplied':
        return 'na isti proizvod primenjen je drugi kupon';
      case 'DiscountTooSmall':
        return 'popust je manji od 1 EC';
    }
  }

  protected async enterGiftCard(event: Event): Promise<void> {
    event.preventDefault();
    if (
      this.pending() ||
      this.cart.isLoading() ||
      !this.cart.hasValue() ||
      !this.giftCardForm().valid()
    ) {
      return;
    }
    this.pending.set(true);
    this.applyingGiftCard.set(true);
    this.cartNotice.set(null);
    this.giftCardError.set(null);
    this.removeGiftCardError.set(null);
    this.checkoutError.set(null);
    try {
      await this.shoppingCart.enterGiftCard(this.giftCardForm.giftCardId().value());
      this.giftCardForm().reset({ giftCardId: '' });
      this.cart.reload();
    } catch (error) {
      this.giftCardError.set(serverMessage(error, 'Poklon kartica nije mogla da se primeni.'));
    } finally {
      this.applyingGiftCard.set(false);
      this.pending.set(false);
    }
  }

  protected async removeGiftCard(): Promise<void> {
    if (this.pending() || this.cart.isLoading() || !this.cart.hasValue()) {
      return;
    }
    this.pending.set(true);
    this.removingGiftCard.set(true);
    this.cartNotice.set(null);
    this.giftCardError.set(null);
    this.removeGiftCardError.set(null);
    this.checkoutError.set(null);
    try {
      await this.shoppingCart.removeGiftCard();
      this.cart.reload();
    } catch (error) {
      this.removeGiftCardError.set(serverMessage(error, 'Uklanjanje poklon kartice nije uspelo.'));
    } finally {
      this.removingGiftCard.set(false);
      this.pending.set(false);
    }
  }

  protected giftCardNotAppliedLabel(reason: GiftCardNotAppliedReason): string {
    switch (reason) {
      case 'Expired':
        return 'istekla';
      case 'Redeemed':
        return 'već je iskorišćena';
      case 'NotRecipient':
        return 'nisi primalac kartice';
      case 'TotalIsZero':
        return 'zbir je već 0 EC';
    }
  }

  protected date(iso: string): string {
    return new Date(iso).toLocaleDateString('sr-Latn-RS');
  }

  protected async checkout(): Promise<void> {
    if (this.checkoutBlocked() || !this.cart.hasValue()) {
      return;
    }
    const contents = this.cart.value();
    const seenTotalInEc = contents.totalFromWalletInEc;
    this.pending.set(true);
    this.purchasing.set(true);
    this.cartNotice.set(null);
    this.checkoutError.set(null);
    this.checkoutConfirmed.set(false);
    try {
      await this.shoppingCart.checkout({ seenTotalInEc });
      this.analytics.record('cart_purchased', {
        tour_count: contents.items.length,
        total_in_ec: seenTotalInEc,
        discount_in_ec: contents.totalInEc - contents.discountedTotalInEc,
      });
      this.addError.set(null);
      this.removeError.set(null);
      this.couponError.set(null);
      this.removeCouponError.set(null);
      this.giftCardError.set(null);
      this.removeGiftCardError.set(null);
      this.focusCartTitle();
      this.checkoutConfirmed.set(true);
      this.cart.reload();
      this.wallet.reload();
      this.giftCards.reload();
    } catch (error) {
      this.checkoutError.set(serverMessage(error, 'Kupovina nije uspela.'));
      this.cart.reload();
      this.wallet.reload();
    } finally {
      this.pending.set(false);
      this.purchasing.set(false);
    }
  }

  private focusCartTitle(): void {
    document.getElementById('payment-cart-title')?.focus();
  }
}
