namespace Payment.Application.CouponAuthoring;

public sealed record CreateCouponDto(int DiscountPercent, DateOnly ValidUntil, Guid? ProductId);
