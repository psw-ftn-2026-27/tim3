using Payment.Application.Products;
using Payment.Domain.ShoppingCarts;
using Shared.Domain.Exceptions;
using Cart = Payment.Domain.ShoppingCarts.ShoppingCart;

namespace Payment.Application.ShoppingCart;

public sealed class ShoppingCartService
{
    private readonly IShoppingCartRepository _carts;
    private readonly IProductRepository _products;
    private readonly ITourPurchaseTokenRepository _purchases;
    private readonly CartPurchasing _cartPurchasing;
    private readonly IUnitOfWork _unitOfWork;

    public ShoppingCartService(
        IShoppingCartRepository carts,
        IProductRepository products,
        ITourPurchaseTokenRepository purchases,
        CartPurchasing cartPurchasing,
        IUnitOfWork unitOfWork)
    {
        _carts = carts;
        _products = products;
        _purchases = purchases;
        _cartPurchasing = cartPurchasing;
        _unitOfWork = unitOfWork;
    }

    public async Task AddToCartAsync(Guid userId, Guid tourId)
    {
        var product = await _products.GetByTourIdAsync(tourId)
            ?? throw new NotFoundException("The tour has no product on offer.");
        var existingCart = await _carts.GetByUserIdAsync(userId);
        var alreadyPurchased = await _purchases.ExistsAsync(userId, tourId);
        var cart = existingCart ?? new Cart(userId);

        _cartPurchasing.AddToCart(cart, product, alreadyPurchased);
        if (existingCart is null)
        {
            _carts.Add(cart);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RemoveFromCartAsync(Guid userId, Guid tourId)
    {
        var cart = await _carts.GetByUserIdAsync(userId)
            ?? throw new NotFoundException("The tour is not in the cart.");
        cart.RemoveItem(tourId);
        await _unitOfWork.SaveChangesAsync();
    }
}
