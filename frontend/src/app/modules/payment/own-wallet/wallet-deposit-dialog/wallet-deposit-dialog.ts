import { Component, input, output } from '@angular/core';
import { FieldTree, FormField } from '@angular/forms/signals';

@Component({
  selector: 'app-wallet-deposit-dialog',
  imports: [FormField],
  templateUrl: './wallet-deposit-dialog.html',
  styleUrl: './wallet-deposit-dialog.scss',
})
export class WalletDepositDialog {
  readonly recipient = input.required<string>();
  readonly balance = input.required<number>();
  readonly amountField = input.required<FieldTree<number | null>>();
  readonly valid = input(false);
  readonly blocked = input(false);
  readonly pending = input(false);
  readonly error = input<string | null>(null);
  readonly submitted = output<Event>();
  readonly dismissed = output<void>();

  protected cancel(event: Event): void {
    // Keep an in-flight payment visible; the page closes the native dialog explicitly.
    event.preventDefault();
    if (!this.pending()) this.dismissed.emit();
  }
}
