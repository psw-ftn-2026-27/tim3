import { Component, input, output } from '@angular/core';
import { WalletTransactionDto, WalletTransactionKind } from '../../api/payment-api-types';

@Component({
  selector: 'app-wallet-history',
  templateUrl: './wallet-history.html',
  styleUrl: './wallet-history.scss',
})
export class WalletHistory {
  readonly transactions = input.required<WalletTransactionDto[]>();
  readonly recipient = input<string | null>(null);
  readonly reversalId = input<string | null>(null);
  readonly reversalError = input<string | null>(null);
  readonly disabled = input(false);

  readonly requestReversal = output<string>();
  readonly confirmReversal = output<void>();
  readonly dismissReversal = output<void>();

  private reversalButton: HTMLButtonElement | null = null;

  protected kindLabel(kind: WalletTransactionKind): string {
    switch (kind) {
      case 'Deposit':
        return 'Uplata';
      case 'Reversal':
        return 'Storno uplate';
      case 'Purchase':
        return 'Kupovina ture';
      case 'GiftCardPurchase':
        return 'Kupovina poklon kartice';
    }
  }

  protected date(iso: string): string {
    return new Date(iso).toLocaleString('sr-Latn-RS');
  }

  protected openReversal(event: Event, depositId: string): void {
    const button = event.currentTarget;
    if (button instanceof HTMLButtonElement) {
      this.reversalButton = button;
    }
    this.requestReversal.emit(depositId);
  }

  protected closeReversal(): void {
    this.dismissReversal.emit();
    this.reversalButton?.focus();
  }
}
