import { PageResult } from '../../../shared/api/page-result';

export type WalletTransactionKind = 'Deposit' | 'Reversal' | 'Purchase' | 'GiftCardPurchase';

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

export interface ReceivedGiftCardDto {
  id: string;
  buyerId: string;
  buyerDisplayName: string;
  amount: number;
  message: string | null;
  expiresAt: string;
  status: GiftCardStatus;
}

export interface CouponDto {
  id: string;
  code: string;
  discountPercent: number;
  validUntil: string;
  productId: string | null;
  productName: string | null;
  isExpired: boolean;
}

export interface CreateCouponDto {
  discountPercent: number;
  validUntil: string;
  productId: string | null;
}

export interface ProductForCouponDto {
  id: string;
  tourId: string;
  name: string;
  priceInEc: number;
}
