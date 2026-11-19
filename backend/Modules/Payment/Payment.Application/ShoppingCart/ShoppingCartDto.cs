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

// The chosen gift card: applied for AppliedInEc with UnusedInEc of it lost, or not applied for NotAppliedReason.
public sealed record CartGiftCardDto(
    Guid GiftCardId,
    int AmountInEc,
    bool IsApplied,
    int AppliedInEc,
    int UnusedInEc,
    GiftCardNotAppliedReason? NotAppliedReason);

// DiscountedTotalInEc is the total after coupons; TotalFromWalletInEc is what is left to pay after the gift card.
public sealed record ShoppingCartDto(
    List<OrderItemDto> Items,
    List<CartCouponDto> Coupons,
    CartGiftCardDto? GiftCard,
    long TotalInEc,
    long DiscountedTotalInEc,
    long TotalFromWalletInEc);
