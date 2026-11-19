using Payment.Application.GiftCards;
using Payment.Application.ShoppingCart;
using Payment.Domain.ShoppingCarts;
using Shared.Domain.Exceptions;
using Cart = Payment.Domain.ShoppingCarts.ShoppingCart;

namespace Payment.Application.CartGiftCard;

public sealed class CartGiftCardService
{
    private readonly IShoppingCartRepository _carts;
    private readonly IGiftCardRepository _giftCards;
    private readonly CartPurchasing _cartPurchasing;
    private readonly IUnitOfWork _unitOfWork;

    public CartGiftCardService(
        IShoppingCartRepository carts,
        IGiftCardRepository giftCards,
        CartPurchasing cartPurchasing,
        IUnitOfWork unitOfWork)
    {
        _carts = carts;
        _giftCards = giftCards;
        _cartPurchasing = cartPurchasing;
        _unitOfWork = unitOfWork;
    }

    public async Task EnterGiftCardAsync(Guid userId, Guid giftCardId)
    {
        var giftCard = await _giftCards.GetByIdAsync(giftCardId)
            ?? throw new NotFoundException("The gift card does not exist.");
        var existingCart = await _carts.GetByUserIdAsync(userId);
        var cart = existingCart ?? new Cart(userId);

        _cartPurchasing.EnterGiftCard(cart, giftCard, DateTime.UtcNow);
        if (existingCart is null)
        {
            _carts.Add(cart);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RemoveGiftCardAsync(Guid userId)
    {
        var cart = await _carts.GetByUserIdAsync(userId)
            ?? throw new NotFoundException("The cart has no gift card.");
        cart.RemoveGiftCard();
        await _unitOfWork.SaveChangesAsync();
    }
}
