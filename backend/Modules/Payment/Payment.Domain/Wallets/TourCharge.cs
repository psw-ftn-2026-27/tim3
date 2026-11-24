namespace Payment.Domain.Wallets;

// The charge for one tour: its price in the cart, the coupon discount taken off it and the part paid by a gift card.
public sealed record TourCharge(Guid TourId, int PriceInEc, int DiscountInEc, int GiftCardInEc)
{
    public int FromWalletInEc => PriceInEc - DiscountInEc - GiftCardInEc;
}
