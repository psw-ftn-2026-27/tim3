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
            throw new DomainException("You already own this tour.");
        }
    }
}
