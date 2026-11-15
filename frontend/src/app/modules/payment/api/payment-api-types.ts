import { PageResult } from '../../../shared/api/page-result';

export type WalletTransactionKind = 'Deposit' | 'Reversal' | 'GiftCardPurchase';

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

export type GiftCardStatus = 'Valid' | 'Expired' | 'Redeemed';

export interface PurchasedGiftCardDto {
  id: string;
  recipientId: string;
  recipientDisplayName: string;
  amount: number;
  message: string | null;
  expiresAt: string;
  status: GiftCardStatus;
}

export interface RecipientDto {
  userId: string;
  displayName: string;
}

export interface BuyGiftCardDto {
  recipientId: string;
  amount: number;
  message: string | null;
}
