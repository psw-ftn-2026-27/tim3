using Payment.Domain.Coupons;
using Payment.Domain.GiftCards;
using Payment.Domain.Products;
using Payment.Domain.TourPurchases;
using Payment.Domain.Wallets;
using Shared.Domain.Exceptions;

namespace Payment.Domain.ShoppingCarts;

public sealed class CartPurchasing
{
    private readonly CartPricing _pricing;

    public CartPurchasing(CartPricing pricing)
    {
        _pricing = pricing;
    }

    public void AddToCart(ShoppingCart cart, Product product, bool alreadyPurchased)
    {
        EnsureNotPurchased(alreadyPurchased);

        cart.AddItem(product);
    }

    // A coupon that loses to a better one on its item (P3) is still accepted and shown as not applied.
    public void EnterCoupon(ShoppingCart cart, Coupon coupon, IReadOnlySet<Guid> usedCouponIds, DateOnly today)
    {
        if (coupon.IsExpiredOn(today))
        {
            throw new DomainException("The coupon has expired.");
        }

        if (usedCouponIds.Contains(coupon.Id))
        {
            throw new DomainException("You have already used this coupon.");
        }

        if (CouponAllocation.FindTargetItem(cart, coupon) is null)
        {
            throw new DomainException("The cart has no product this coupon applies to.");
        }

        cart.EnterCoupon(coupon.Id);
    }

    // P1 when choosing: only the recipient's valid card can be chosen.
    public void EnterGiftCard(ShoppingCart cart, GiftCard giftCard, DateTime moment)
    {
        if (!giftCard.CanBeUsedBy(cart.UserId, moment))
        {
            throw new DomainException("The gift card is not yours or is no longer valid.");
        }

        cart.EnterGiftCard(giftCard.Id);
    }

    public IReadOnlyList<TourPurchaseToken> Checkout(
        ShoppingCart cart,
        Wallet wallet,
        IReadOnlyCollection<Coupon> coupons,
        GiftCard? giftCard,
        BuyerHistory history,
        IReadOnlySet<Guid> availableTourIds,
        long seenTotalInEc,
        DateTime purchasedAt)
    {
        ValidateCheckout(cart, wallet, availableTourIds, history.PurchasedTourIds);

        var price = _pricing.Price(cart, coupons, history.UsedCouponIds, giftCard, purchasedAt);
        if (price.TotalFromWalletInEc != seenTotalInEc)
        {
            throw new DomainException("The cart total has changed. Review the cart and confirm the purchase again.");
        }

        var tokens = cart.Items
            .Select(item => new TourPurchaseToken(
                cart.UserId,
                item.TourId,
                purchasedAt,
                price.Coupons.AppliedTo(item.TourId)?.CouponId,
                price.DiscountOn(item.TourId)))
            .ToList();
        var charges = cart.Items
            .Select(item => new TourCharge(
                item.TourId, item.PriceInEc, price.DiscountOn(item.TourId), price.GiftCardOn(item.TourId)))
            .ToList();
        wallet.ChargeForTours(charges, purchasedAt);
        // P5: only a card that was applied is spent; one left in the cart unapplied stays as it is.
        if (price.GiftCard is { IsApplied: true })
        {
            giftCard!.Redeem(cart.UserId, purchasedAt);
        }

        cart.ClearAfterPurchase();
        return tokens;
    }

    private static void ValidateCheckout(
        ShoppingCart cart,
        Wallet wallet,
        IReadOnlySet<Guid> availableTourIds,
        IReadOnlySet<Guid> purchasedTourIds)
    {
        if (cart.Items.Count == 0)
        {
            throw new DomainException("The cart is empty.");
        }

        if (cart.UserId != wallet.UserId)
        {
            throw new DomainException("The cart and wallet must belong to the same buyer.");
        }

        foreach (var item in cart.Items)
        {
            if (!availableTourIds.Contains(item.TourId))
            {
                throw new DomainException($"The tour '{item.Name}' is no longer available.");
            }

            EnsureNotPurchased(purchasedTourIds.Contains(item.TourId));
        }
    }

    private static void EnsureNotPurchased(bool alreadyPurchased)
    {
        if (alreadyPurchased)
        {
            throw new DomainException("You already own this tour.");
        }
    }
}
