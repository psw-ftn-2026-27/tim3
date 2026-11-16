import { PageResult } from '../../../shared/api/page-result';

export type WalletTransactionKind = 'Deposit' | 'Reversal' | 'Purchase';

export interface WalletTransactionDto {
  id: string;
  amount: number;
  occurredAt: string;
  kind: WalletTransactionKind;
  tourId: string | null;
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

export interface OrderItemDto {
  productId: string;
  tourId: string;
  name: string;
  priceInEc: number;
}

export interface ShoppingCartDto {
  items: OrderItemDto[];
  totalInEc: number;
}

export interface ProductForCartDto {
  tourId: string;
  name: string;
  priceInEc: number;
}
