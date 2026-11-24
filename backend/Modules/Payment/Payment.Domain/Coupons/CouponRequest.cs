namespace Payment.Domain.Coupons;

// What the author asks for: an optional product (null means all of the author's products), the discount and the last valid day.
public sealed record CouponRequest(Guid? ProductId, int DiscountPercent, DateOnly ValidUntil);
