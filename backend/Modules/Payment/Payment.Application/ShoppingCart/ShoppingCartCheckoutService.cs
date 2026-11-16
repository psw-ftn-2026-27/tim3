using Payment.Application.Wallets;
using Payment.Domain.ShoppingCarts;
using Payment.Domain.Wallets;
using Shared.Domain.Exceptions;

namespace Payment.Application.ShoppingCart;

public sealed class ShoppingCartCheckoutService
{
    private readonly IShoppingCartRepository _carts;
    private readonly IWalletRepository _wallets;
    private readonly ITourPurchaseTokenRepository _purchases;
    private readonly ITourAvailability _availability;
    private readonly CartPurchasing _purchasing;
    private readonly IUnitOfWork _unitOfWork;

    public ShoppingCartCheckoutService(
        IShoppingCartRepository carts,
        IWalletRepository wallets,
        ITourPurchaseTokenRepository purchases,
        ITourAvailability availability,
        CartPurchasing purchasing,
        IUnitOfWork unitOfWork)
    {
        _carts = carts;
        _wallets = wallets;
        _purchases = purchases;
        _availability = availability;
        _purchasing = purchasing;
        _unitOfWork = unitOfWork;
    }

    public async Task CheckoutAsync(Guid userId)
    {
        var cart = await _carts.GetByUserIdAsync(userId)
            ?? throw new DomainException("Korpa je prazna.");
        var availableTourIds = new HashSet<Guid>();
        var purchasedTourIds = new HashSet<Guid>();
        foreach (var item in cart.Items)
        {
            if (await _availability.IsAvailableAsync(item.TourId))
            {
                availableTourIds.Add(item.TourId);
            }

            if (await _purchases.ExistsAsync(userId, item.TourId))
            {
                purchasedTourIds.Add(item.TourId);
            }
        }

        var existingWallet = await _wallets.GetByUserIdAsync(userId);
        var wallet = existingWallet ?? new Wallet(userId);
        var tokens = _purchasing.Checkout(cart, wallet, availableTourIds, purchasedTourIds, DateTime.UtcNow);
        if (existingWallet is null)
        {
            _wallets.Add(wallet);
        }

        foreach (var token in tokens)
        {
            _purchases.Add(token);
        }

        await _unitOfWork.SaveChangesAsync();
    }
}
