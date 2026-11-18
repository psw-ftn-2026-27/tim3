using Payment.Application.Coupons;
using Payment.Application.GiftCards;
using Payment.Application.Products;
using Payment.Domain.Coupons;
using Payment.Domain.ShoppingCarts;
using Shared.Domain.Exceptions;
using Cart = Payment.Domain.ShoppingCarts.ShoppingCart;

namespace Payment.Application.ShoppingCart;

public sealed class ShoppingCartQueries
{
    private readonly IShoppingCartRepository _carts;
    private readonly ICouponRepository _coupons;
    private readonly IGiftCardRepository _giftCards;
    private readonly ITourPurchaseTokenRepository _purchases;
    private readonly IProductReadRepository _products;
    private readonly CartPricing _pricing;

    public ShoppingCartQueries(
        IShoppingCartRepository carts,
        ICouponRepository coupons,
        IGiftCardRepository giftCards,
        ITourPurchaseTokenRepository purchases,
        IProductReadRepository products,
        CartPricing pricing)
    {
        _carts = carts;
        _coupons = coupons;
        _giftCards = giftCards;
        _purchases = purchases;
        _products = products;
        _pricing = pricing;
    }

    // Discounts depend on the current moment, on the coupons the buyer has used and on the state of the gift card,
    // so they are computed, never stored.
    public async Task<ShoppingCartDto> GetMyCartAsync(Guid userId)
    {
        var cart = await _carts.GetByUserIdAsync(userId);
        if (cart is null)
        {
            return new ShoppingCartDto([], [], null, 0, 0, 0);
        }

        var coupons = await _coupons.GetByIdsAsync(cart.CouponIds);
        var giftCard = cart.GiftCardId is null ? null : await _giftCards.GetByIdAsync(cart.GiftCardId.Value);
        var usedCouponIds = (await _purchases.GetUsedCouponIdsAsync(userId)).ToHashSet();
        var price = _pricing.Price(cart, coupons, usedCouponIds, giftCard, DateTime.UtcNow);
        return ToDto(cart, coupons, price);
    }

    public async Task<ProductForCartDto> GetProductForCartAsync(Guid tourId)
    {
        return await _products.GetProductForCartAsync(tourId)
            ?? throw new NotFoundException("The tour has no product on offer.");
    }

    private static ShoppingCartDto ToDto(Cart cart, List<Coupon> coupons, CartPrice price)
    {
        var items = cart.Items
            .Select(item => new OrderItemDto(
                item.ProductId,
                item.TourId,
                item.Name,
                item.PriceInEc,
                item.PriceInEc - price.DiscountOn(item.TourId)))
            .ToList();
        return new ShoppingCartDto(
            items,
            ToCouponDtos(cart, coupons, price),
            ToGiftCardDto(price.GiftCard),
            cart.TotalInEc,
            price.TotalAfterCouponsInEc,
            price.TotalFromWalletInEc);
    }

    private static List<CartCouponDto> ToCouponDtos(Cart cart, List<Coupon> coupons, CartPrice price)
    {
        return price.Coupons.Coupons
            .Select(result => new CartCouponDto(
                result.CouponId,
                coupons.Single(coupon => coupon.Id == result.CouponId).Code.Value,
                result.TourId is not null,
                result.TourId,
                cart.Items.SingleOrDefault(item => item.TourId == result.TourId)?.Name,
                result.DiscountInEc,
                result.Reason))
            .ToList();
    }

    private static CartGiftCardDto? ToGiftCardDto(PricedGiftCard? giftCard)
    {
        return giftCard is null
            ? null
            : new CartGiftCardDto(
                giftCard.GiftCardId,
                giftCard.AmountInEc,
                giftCard.IsApplied,
                giftCard.AppliedInEc,
                giftCard.UnusedInEc,
                giftCard.Reason);
    }
}
