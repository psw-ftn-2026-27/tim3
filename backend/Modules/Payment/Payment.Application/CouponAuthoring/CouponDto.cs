namespace Payment.Application.CouponAuthoring;

public sealed record CouponDto(
    Guid Id, string Code, int DiscountPercent, DateOnly ValidUntil, Guid? ProductId, string? ProductName, bool IsExpired);
