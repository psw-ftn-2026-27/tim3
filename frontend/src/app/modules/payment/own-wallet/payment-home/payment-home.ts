import { HttpErrorResponse, httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { form, max, min, required, validate } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { PageResult } from '../../../../shared/api/page-result';
import { serverMessage } from '../../../../shared/util/server-message';
import { UserWalletSummaryDto, WalletDto } from '../../api/payment-api-types';
import { OwnWallet } from '../own-wallet';
import { PaymentNavigation } from '../payment-navigation/payment-navigation';
import { WalletHistory } from '../wallet-history/wallet-history';
import { WalletPagination } from '../wallet-pagination/wallet-pagination';
import { WalletUserList } from '../wallet-user-list/wallet-user-list';
import { WalletDepositDialog } from '../wallet-deposit-dialog/wallet-deposit-dialog';

const BASE_URL = '/api/payment/wallet';
const MAX_EC_AMOUNT = 2_147_483_647;

@Component({
  imports: [
    RouterLink,
    PaymentNavigation,
    WalletHistory,
    WalletPagination,
    WalletUserList,
    WalletDepositDialog,
  ],
  selector: 'app-payment-home',
  templateUrl: './payment-home.html',
  styleUrl: './payment-home.scss',
})
export class PaymentHome {
  protected readonly auth = inject(Auth);
  private readonly wallet = inject(OwnWallet);

  protected readonly pageSize = 20;
  protected readonly ownPage = signal(1);
  protected readonly usersPage = signal(1);
  protected readonly historyPage = signal(1);

  protected readonly isAdministrator = computed(
    () => this.auth.user()?.roles.includes('administrator') ?? false,
  );
  protected readonly isExplorer = computed(
    () => this.auth.user()?.roles.includes('explorer') ?? false,
  );

  protected readonly selectedUser = signal<UserWalletSummaryDto | null>(null);
  protected readonly reversalId = signal<string | null>(null);
  protected readonly pending = signal(false);
  private depositDialog: HTMLDialogElement | null = null;
  private depositTrigger: HTMLButtonElement | null = null;

  protected readonly depositError = signal<string | null>(null);
  protected readonly reversalError = signal<string | null>(null);
  protected readonly confirmation = signal<string | null>(null);

  private readonly searchText = signal('');
  protected readonly searchForm = form(signal({ text: '' }));
  protected readonly depositForm = form(
    signal<{ amount: number | null }>({ amount: null }),
    (path) => {
      required(path.amount, { message: 'Unesi iznos u EC.' });
      min(path.amount, 1, { message: 'Iznos mora biti veći od nule.' });
      max(path.amount, MAX_EC_AMOUNT, { message: 'Iznos ne može biti veći od 2.147.483.647 EC.' });
      validate(path.amount, ({ value }) => {
        const amount = value();
        return amount === null || Number.isInteger(amount)
          ? null
          : { kind: 'integer', message: 'Unesi ceo broj EC.' };
      });
    },
  );

  protected readonly ownWallet = httpResource<WalletDto>(() =>
    this.auth.isLoggedIn()
      ? `${BASE_URL}/me?page=${this.ownPage()}&pageSize=${this.pageSize}`
      : undefined,
  );
  protected readonly ownWalletErrorStatus = computed(() => {
    const failure = this.ownWallet.error();
    return failure instanceof HttpErrorResponse ? failure.status : null;
  });

  protected readonly users = httpResource<PageResult<UserWalletSummaryDto>>(() =>
    this.isAdministrator()
      ? `${BASE_URL}/users?text=${encodeURIComponent(this.searchText())}&page=${this.usersPage()}&pageSize=${this.pageSize}`
      : undefined,
  );
  protected readonly usersErrorStatus = computed(() => {
    const failure = this.users.error();
    return failure instanceof HttpErrorResponse ? failure.status : null;
  });

  protected readonly selectedWallet = httpResource<WalletDto>(() => {
    const user = this.selectedUser();
    return this.isAdministrator() && user
      ? `${BASE_URL}/${user.userId}?page=${this.historyPage()}&pageSize=${this.pageSize}`
      : undefined;
  });
  protected readonly selectedWalletErrorStatus = computed(() => {
    const failure = this.selectedWallet.error();
    return failure instanceof HttpErrorResponse ? failure.status : null;
  });

  protected readonly writeBlocked = computed(
    () =>
      this.pending() ||
      this.ownWallet.isLoading() ||
      this.users.isLoading() ||
      this.selectedWallet.isLoading(),
  );

  protected openDeposit(event: Event): void {
    if (this.writeBlocked() || !this.selectedWallet.hasValue() || this.selectedWallet.error())
      return;
    const trigger = event.currentTarget;
    if (!(trigger instanceof HTMLButtonElement)) return;
    const dialog = trigger.closest('section')?.querySelector('dialog');
    if (!(dialog instanceof HTMLDialogElement)) return;
    this.depositForm().reset({ amount: null });
    this.depositError.set(null);
    this.confirmation.set(null);
    this.cancelReversal();
    this.depositTrigger = trigger;
    this.depositDialog = dialog;
    // showModal gives native focus containment, background inertness and autofocus.
    dialog.showModal();
  }

  protected closeDeposit(): void {
    if (this.pending()) return;
    this.depositDialog?.close();
    this.depositTrigger?.focus();
    this.depositDialog = null;
  }

  protected backToUsers(): void {
    document.getElementById('wallet-users-title')?.focus();
  }

  protected search(event: Event): void {
    event.preventDefault();
    if (this.pending()) {
      return;
    }
    this.usersPage.set(1);
    this.searchText.set(this.searchForm.text().value().trim());
  }

  protected selectUser(user: UserWalletSummaryDto): void {
    if (this.pending()) {
      return;
    }
    this.selectedUser.set(user);
    document.getElementById('wallet-detail-title')?.focus();
    this.historyPage.set(1);
    this.depositForm().reset({ amount: null });
    this.depositError.set(null);
    this.confirmation.set(null);
    this.cancelReversal();
  }

  protected selectHistoryPage(page: number): void {
    this.cancelReversal();
    this.historyPage.set(page);
  }

  protected startReversal(depositId: string): void {
    this.reversalError.set(null);
    this.confirmation.set(null);
    this.reversalId.set(depositId);
  }

  protected cancelReversal(): void {
    this.reversalId.set(null);
    this.reversalError.set(null);
  }

  protected async deposit(event: Event): Promise<void> {
    event.preventDefault();
    const user = this.selectedUser();
    const amount = this.depositForm.amount().value();
    if (
      !this.isAdministrator() ||
      this.writeBlocked() ||
      !user ||
      amount === null ||
      !this.depositForm().valid()
    ) {
      return;
    }
    this.pending.set(true);
    this.depositError.set(null);
    this.confirmation.set(null);
    this.cancelReversal();
    try {
      await this.wallet.deposit(user.userId, { amount });
      this.depositForm().reset({ amount: null });
      this.depositDialog?.close();
      document.getElementById('wallet-detail-title')?.focus();
      this.depositDialog = null;
      this.confirmation.set('Uplata je uspešno izvršena.');
      this.refreshWallets();
    } catch (failure) {
      this.depositError.set(serverMessage(failure, 'Uplata trenutno nije moguća.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async reverse(): Promise<void> {
    const user = this.selectedUser();
    const depositId = this.reversalId();
    if (!this.isAdministrator() || this.writeBlocked() || !user || !depositId) {
      return;
    }
    this.pending.set(true);
    this.reversalError.set(null);
    this.confirmation.set(null);
    try {
      await this.wallet.reverse(user.userId, depositId);
      document.getElementById('wallet-detail-title')?.focus();
      this.cancelReversal();
      this.confirmation.set(`Uplata za korisnika ${user.displayName} je stornirana.`);
      this.refreshWallets();
    } catch (failure) {
      this.reversalError.set(serverMessage(failure, 'Storno trenutno nije moguć.'));
    } finally {
      this.pending.set(false);
    }
  }

  private refreshWallets(): void {
    this.ownPage.set(1);
    this.historyPage.set(1);
    this.ownWallet.reload();
    this.users.reload();
    this.selectedWallet.reload();
  }
}
