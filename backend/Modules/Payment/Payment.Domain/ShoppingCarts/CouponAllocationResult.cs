namespace Payment.Domain.ShoppingCarts;

// One entered coupon: either applied to the item with TourId for DiscountInEc, or not applied for Reason.
public sealed record AllocatedCoupon(Guid CouponId, Guid? TourId, int DiscountInEc, CouponNotAppliedReason? Reason);

public sealed record CouponAllocationResult(IReadOnlyList<AllocatedCoupon> Coupons, long TotalInEc)
{
    public AllocatedCoupon? AppliedTo(Guid tourId)
    {
        return Coupons.SingleOrDefault(coupon => coupon.TourId == tourId);
    }
}
