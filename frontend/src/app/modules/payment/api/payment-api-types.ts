import { PageResult } from '../../../shared/api/page-result';

export type WalletTransactionKind = 'Deposit' | 'Reversal';

export interface WalletTransactionDto {
  id: string;
  amount: number;
  occurredAt: string;
  kind: WalletTransactionKind;
}

export interface WalletDto {
  balance: number;
  transactions: PageResult<WalletTransactionDto>;
}

export interface UserWalletSummaryDto {
  userId: string;
  displayName: string;
  balance: number;
}

export interface DepositEcDto {
  amount: number;
}
