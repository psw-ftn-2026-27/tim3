using Payment.Domain.ShoppingCarts;

namespace Payment.Application.ShoppingCart;

public sealed record OrderItemDto(Guid ProductId, Guid TourId, string Name, int PriceInEc, int DiscountedPriceInEc);

// An entered coupon: applied to TourId for DiscountInEc, or not applied for NotAppliedReason.
public sealed record CartCouponDto(
    Guid CouponId,
    string Code,
    bool IsApplied,
    Guid? TourId,
    string? TourName,
    int DiscountInEc,
    CouponNotAppliedReason? NotAppliedReason);

public sealed record ShoppingCartDto(
    List<OrderItemDto> Items, List<CartCouponDto> Coupons, long TotalInEc, long DiscountedTotalInEc);
