using Payment.Domain.Products;
using Payment.Domain.TourPurchases;
using Payment.Domain.Wallets;
using Shared.Domain.Exceptions;

namespace Payment.Domain.ShoppingCarts;

public sealed class CartPurchasing
{
    public void AddToCart(ShoppingCart cart, Product product, bool alreadyPurchased)
    {
        EnsureNotPurchased(alreadyPurchased);

        cart.AddItem(product);
    }

    public IReadOnlyList<TourPurchaseToken> Checkout(
        ShoppingCart cart,
        Wallet wallet,
        IReadOnlySet<Guid> availableTourIds,
        IReadOnlySet<Guid> purchasedTourIds,
        DateTime purchasedAt)
    {
        if (cart.Items.Count == 0)
        {
            throw new DomainException("Korpa je prazna.");
        }

        if (cart.UserId != wallet.UserId)
        {
            throw new DomainException("Korpa i novčanik moraju pripadati istom kupcu.");
        }

        foreach (var item in cart.Items)
        {
            if (!availableTourIds.Contains(item.TourId))
            {
                throw new DomainException($"Tura „{item.Name}“ više nije dostupna.");
            }

            EnsureNotPurchased(purchasedTourIds.Contains(item.TourId));
        }

        var tokens = cart.Items
            .Select(item => new TourPurchaseToken(cart.UserId, item.TourId, purchasedAt))
            .ToList();
        wallet.ChargeForTours(cart.Items, purchasedAt);
        cart.ClearAfterPurchase();
        return tokens;
    }

    private static void EnsureNotPurchased(bool alreadyPurchased)
    {
        if (alreadyPurchased)
        {
            throw new DomainException("Već poseduješ ovu turu.");
        }
    }
}
