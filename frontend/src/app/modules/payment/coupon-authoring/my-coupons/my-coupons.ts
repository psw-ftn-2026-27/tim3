import { HttpErrorResponse, httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { FormField, form, max, min, required, validate } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { serverMessage } from '../../../../shared/util/server-message';
import { CouponDto, ProductForCouponDto } from '../../api/payment-api-types';
import { PaymentNavigation } from '../../own-wallet/payment-navigation/payment-navigation';
import { CouponAuthoring } from '../coupon-authoring';
import { CouponList } from '../coupon-list/coupon-list';

const BASE_URL = '/api/payment/coupons';
const ALL_PRODUCTS = '';

@Component({
  imports: [CouponList, FormField, PaymentNavigation, RouterLink],
  selector: 'app-my-coupons',
  templateUrl: './my-coupons.html',
  styleUrl: './my-coupons.scss',
})
export class MyCoupons {
  protected readonly auth = inject(Auth);
  private readonly authoring = inject(CouponAuthoring);

  protected readonly allProducts = ALL_PRODUCTS;
  protected readonly isExplorer = computed(
    () => this.auth.user()?.roles.includes('explorer') ?? false,
  );
  protected readonly showForm = signal(false);
  protected readonly pending = signal(false);
  protected readonly createError = signal<string | null>(null);
  protected readonly confirmation = signal<string | null>(null);

  protected readonly createForm = form(
    signal<{ discountPercent: number | null; validUntil: string; productId: string }>({
      discountPercent: null,
      validUntil: '',
      productId: ALL_PRODUCTS,
    }),
    (path) => {
      required(path.discountPercent, { message: 'Unesi procenat popusta.' });
      min(path.discountPercent, 1, { message: 'Popust mora biti najmanje 1%.' });
      max(path.discountPercent, 100, { message: 'Popust može biti najviše 100%.' });
      validate(path.discountPercent, ({ value }) => {
        const percent = value();
        return percent === null || Number.isInteger(percent)
          ? null
          : { kind: 'integer', message: 'Unesi ceo broj.' };
      });
      required(path.validUntil, { message: 'Unesi datum do kog kupon važi.' });
      validate(path.validUntil, ({ value }) => {
        const validUntil = value();
        // The server compares with today's UTC date, so the form does the same.
        return validUntil === '' || validUntil >= todayUtc()
          ? null
          : { kind: 'past', message: 'Rok ne može biti pre današnjeg dana.' };
      });
    },
  );

  protected readonly coupons = httpResource<CouponDto[]>(() =>
    this.isExplorer() ? BASE_URL : undefined,
  );
  protected readonly couponsErrorStatus = computed(() => {
    const failure = this.coupons.error();
    return failure instanceof HttpErrorResponse ? failure.status : null;
  });
  protected readonly hasCoupons = computed(
    () => this.coupons.hasValue() && this.coupons.value().length > 0,
  );

  protected readonly products = httpResource<ProductForCouponDto[]>(() =>
    this.isExplorer() && this.showForm() ? `${BASE_URL}/products` : undefined,
  );
  protected readonly hasProducts = computed(
    () => this.products.hasValue() && this.products.value().length > 0,
  );

  protected openForm(): void {
    this.confirmation.set(null);
    this.createError.set(null);
    this.showForm.set(true);
  }

  protected closeForm(): void {
    this.createError.set(null);
    this.showForm.set(false);
    this.createForm().reset({ discountPercent: null, validUntil: '', productId: ALL_PRODUCTS });
  }

  protected async create(event: Event): Promise<void> {
    event.preventDefault();
    const discountPercent = this.createForm.discountPercent().value();
    if (
      this.pending() ||
      !this.hasProducts() ||
      discountPercent === null ||
      !this.createForm().valid()
    ) {
      return;
    }
    const productId = this.createForm.productId().value();
    this.pending.set(true);
    this.createError.set(null);
    this.confirmation.set(null);
    try {
      await this.authoring.create({
        discountPercent,
        validUntil: this.createForm.validUntil().value(),
        productId: productId === ALL_PRODUCTS ? null : productId,
      });
      this.closeForm();
      this.confirmation.set('Kupon je napravljen i nalazi se na vrhu spiska.');
      this.coupons.reload();
    } catch (failure) {
      this.createError.set(serverMessage(failure, 'Kupon trenutno nije moguće napraviti.'));
    } finally {
      this.pending.set(false);
    }
  }
}

function todayUtc(): string {
  return new Date().toISOString().slice(0, 10);
}
