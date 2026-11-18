using Payment.Application.Coupons;
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
    private readonly ITourPurchaseTokenRepository _purchases;
    private readonly IProductReadRepository _products;
    private readonly CouponAllocation _allocation;

    public ShoppingCartQueries(
        IShoppingCartRepository carts,
        ICouponRepository coupons,
        ITourPurchaseTokenRepository purchases,
        IProductReadRepository products,
        CouponAllocation allocation)
    {
        _carts = carts;
        _coupons = coupons;
        _purchases = purchases;
        _products = products;
        _allocation = allocation;
    }

    // Discounts depend on the current date and on the coupons the buyer has used, so they are computed, never stored.
    public async Task<ShoppingCartDto> GetMyCartAsync(Guid userId)
    {
        var cart = await _carts.GetByUserIdAsync(userId);
        if (cart is null)
        {
            return new ShoppingCartDto([], [], 0, 0);
        }

        var coupons = await _coupons.GetByIdsAsync(cart.CouponIds);
        var usedCouponIds = (await _purchases.GetUsedCouponIdsAsync(userId)).ToHashSet();
        var allocation = _allocation.Allocate(cart, coupons, usedCouponIds, DateOnly.FromDateTime(DateTime.UtcNow));
        return ToDto(cart, coupons, allocation);
    }

    public async Task<ProductForCartDto> GetProductForCartAsync(Guid tourId)
    {
        return await _products.GetProductForCartAsync(tourId)
            ?? throw new NotFoundException("The tour has no product on offer.");
    }

    private static ShoppingCartDto ToDto(Cart cart, List<Coupon> coupons, CouponAllocationResult allocation)
    {
        var items = cart.Items
            .Select(item => new OrderItemDto(
                item.ProductId,
                item.TourId,
                item.Name,
                item.PriceInEc,
                item.PriceInEc - (allocation.AppliedTo(item.TourId)?.DiscountInEc ?? 0)))
            .ToList();
        var enteredCoupons = allocation.Coupons
            .Select(result => new CartCouponDto(
                result.CouponId,
                coupons.Single(coupon => coupon.Id == result.CouponId).Code.Value,
                result.TourId is not null,
                result.TourId,
                cart.Items.SingleOrDefault(item => item.TourId == result.TourId)?.Name,
                result.DiscountInEc,
                result.Reason))
            .ToList();
        return new ShoppingCartDto(items, enteredCoupons, cart.TotalInEc, allocation.TotalInEc);
    }
}
