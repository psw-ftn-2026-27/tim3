import { HttpErrorResponse, httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { FormField, form, max, maxLength, min, required, validate } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { PageResult } from '../../../../shared/api/page-result';
import { PurchasedGiftCardDto, RecipientDto, WalletDto } from '../../api/payment-api-types';
import { GiftCardPurchasing } from '../gift-card-purchasing';
import { PurchasedGiftCardList } from '../purchased-gift-card-list/purchased-gift-card-list';
import { RecipientList } from '../recipient-list/recipient-list';

const BASE_URL = '/api/payment/gift-cards';
const WALLET_URL = '/api/payment/wallet/me';
const MIN_GIFT_CARD_AMOUNT = 10;
const MAX_GIFT_CARD_AMOUNT = 1000;
const MAX_MESSAGE_LENGTH = 200;
const UNEXPECTED_ERROR = 'Kupovina trenutno nije moguća, pokušaj ponovo.';

@Component({
  imports: [FormField, PurchasedGiftCardList, RecipientList, RouterLink],
  selector: 'app-gift-cards',
  templateUrl: './gift-cards.html',
  styleUrl: './gift-cards.scss',
})
export class GiftCards {
  protected readonly auth = inject(Auth);
  private readonly purchasing = inject(GiftCardPurchasing);

  protected readonly recipientsPageSize = 20;
  protected readonly isExplorer = computed(
    () => this.auth.user()?.roles.includes('explorer') ?? false,
  );
  protected readonly showForm = signal(false);
  protected readonly selectedRecipient = signal<RecipientDto | null>(null);
  protected readonly pending = signal(false);
  protected readonly buyError = signal<string | null>(null);
  protected readonly confirmation = signal<string | null>(null);

  private readonly searchText = signal<string | null>(null);
  protected readonly searchForm = form(signal({ text: '' }));

  protected readonly wallet = httpResource<WalletDto>(() =>
    this.isExplorer() ? WALLET_URL : undefined,
  );
  protected readonly needsWallet = computed(() => {
    const failure = this.wallet.error();
    return failure instanceof HttpErrorResponse && failure.status === 404;
  });
  protected readonly balance = computed(() => {
    if (this.needsWallet()) {
      return 0;
    }
    return this.wallet.hasValue() ? this.wallet.value().balance : null;
  });
  protected readonly maxAmount = computed(() =>
    Math.min(MAX_GIFT_CARD_AMOUNT, this.balance() ?? 0),
  );
  protected readonly canBuy = computed(() => this.maxAmount() >= MIN_GIFT_CARD_AMOUNT);

  protected readonly buyForm = form(
    signal<{ amount: number | null; message: string }>({ amount: null, message: '' }),
    (path) => {
      required(path.amount, { message: 'Unesi iznos u EC.' });
      min(path.amount, MIN_GIFT_CARD_AMOUNT, { message: 'Iznos mora biti najmanje 10 EC.' });
      max(path.amount, () => this.maxAmount(), {
        message: 'Iznos je veći od najvećeg dozvoljenog iznosa.',
      });
      validate(path.amount, ({ value }) => {
        const amount = value();
        return amount === null || Number.isInteger(amount)
          ? null
          : { kind: 'integer', message: 'Unesi ceo broj EC.' };
      });
      maxLength(path.message, MAX_MESSAGE_LENGTH, {
        message: 'Poruka može imati najviše 200 karaktera.',
      });
    },
  );

  protected readonly purchased = httpResource<PurchasedGiftCardDto[]>(() =>
    this.isExplorer() ? `${BASE_URL}/purchased` : undefined,
  );

  protected readonly recipients = httpResource<PageResult<RecipientDto>>(() => {
    const text = this.searchText();
    return this.isExplorer() && this.showForm() && text !== null
      ? `${BASE_URL}/recipients?text=${encodeURIComponent(text)}&page=1&pageSize=${this.recipientsPageSize}`
      : undefined;
  });

  protected readonly writeBlocked = computed(
    () => this.pending() || this.wallet.isLoading() || this.purchased.isLoading(),
  );

  protected openForm(): void {
    this.confirmation.set(null);
    this.buyError.set(null);
    this.showForm.set(true);
  }

  protected search(event: Event): void {
    event.preventDefault();
    if (this.pending()) {
      return;
    }
    this.searchText.set(this.searchForm.text().value().trim());
  }

  protected selectRecipient(recipient: RecipientDto): void {
    if (this.pending()) {
      return;
    }
    this.buyError.set(null);
    this.selectedRecipient.set(recipient);
  }

  protected async buy(event: Event): Promise<void> {
    event.preventDefault();
    const recipient = this.selectedRecipient();
    const amount = this.buyForm.amount().value();
    if (
      this.writeBlocked() ||
      !this.canBuy() ||
      !recipient ||
      amount === null ||
      !this.buyForm().valid()
    ) {
      return;
    }
    const message = this.buyForm.message().value().trim();
    this.pending.set(true);
    this.buyError.set(null);
    this.confirmation.set(null);
    try {
      await this.purchasing.buy({
        recipientId: recipient.userId,
        amount,
        message: message === '' ? null : message,
      });
      this.closeForm();
      this.confirmation.set(
        `Kupljena je poklon kartica od ${amount} EC za korisnika ${recipient.displayName}.`,
      );
      this.purchased.reload();
      this.wallet.reload();
    } catch (failure) {
      this.buyError.set(this.buyErrorMessage(failure));
    } finally {
      this.pending.set(false);
    }
  }

  private buyErrorMessage(failure: unknown): string {
    if (failure instanceof HttpErrorResponse && failure.status === 404) {
      this.selectedRecipient.set(null);
      return 'Izabrani korisnik nije dostupan. Izaberi drugog primaoca.';
    }
    if (failure instanceof HttpErrorResponse && failure.status === 400) {
      this.wallet.reload();
      return 'Nemaš dovoljno EC za karticu ovog iznosa.';
    }
    return UNEXPECTED_ERROR;
  }

  private closeForm(): void {
    this.showForm.set(false);
    this.selectedRecipient.set(null);
    this.searchText.set(null);
    this.searchForm().reset({ text: '' });
    this.buyForm().reset({ amount: null, message: '' });
  }
}
