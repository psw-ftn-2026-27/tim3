namespace Payment.Domain.ShoppingCarts;

public enum CouponNotAppliedReason
{
    Expired,
    AlreadyUsed,
    NoMatchingItem,
    BetterCouponApplied,
    DiscountTooSmall
}
