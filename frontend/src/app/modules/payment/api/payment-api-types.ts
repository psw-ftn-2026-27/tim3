import { PageResult } from '../../../shared/api/page-result';

export type WalletTransactionKind = 'Deposit' | 'Reversal' | 'Purchase' | 'GiftCardPurchase';

export interface WalletTransactionDto {
  id: string;
  amount: number;
  occurredAt: string;
  kind: WalletTransactionKind;
  tourId: string | null;
  discountInEc: number;
  giftCardInEc: number;
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
  discountedPriceInEc: number;
}

export type CouponNotAppliedReason =
  'Expired' | 'AlreadyUsed' | 'NoMatchingItem' | 'BetterCouponApplied' | 'DiscountTooSmall';

export interface CartCouponDto {
  couponId: string;
  code: string;
  isApplied: boolean;
  tourId: string | null;
  tourName: string | null;
  discountInEc: number;
  notAppliedReason: CouponNotAppliedReason | null;
}

export type GiftCardNotAppliedReason = 'Expired' | 'Redeemed' | 'NotRecipient' | 'TotalIsZero';

export interface CartGiftCardDto {
  giftCardId: string;
  amountInEc: number;
  isApplied: boolean;
  appliedInEc: number;
  unusedInEc: number;
  notAppliedReason: GiftCardNotAppliedReason | null;
}

export interface ShoppingCartDto {
  items: OrderItemDto[];
  coupons: CartCouponDto[];
  giftCard: CartGiftCardDto | null;
  totalInEc: number;
  discountedTotalInEc: number;
  totalFromWalletInEc: number;
}

export interface EnterCouponDto {
  code: string;
}

export interface CheckoutDto {
  seenTotalInEc: number;
}

export interface GiftCardForCartDto {
  id: string;
  amount: number;
  expiresAt: string;
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
