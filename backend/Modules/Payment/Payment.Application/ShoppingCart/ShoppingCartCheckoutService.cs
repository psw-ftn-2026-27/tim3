using Payment.Application.Products;
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
            ?? throw new DomainException("The cart is empty.");
        var availableTourIds = await GetAvailableTourIdsAsync(cart.Items);
        var purchasedTourIds = (await _purchases.GetPurchasedTourIdsAsync(userId)).ToHashSet();
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

    private async Task<HashSet<Guid>> GetAvailableTourIdsAsync(IReadOnlyCollection<OrderItem> items)
    {
        var available = new HashSet<Guid>();
        foreach (var item in items)
        {
            if (await _availability.IsAvailableAsync(item.TourId))
            {
                available.Add(item.TourId);
            }
        }
        return available;
    }
}
