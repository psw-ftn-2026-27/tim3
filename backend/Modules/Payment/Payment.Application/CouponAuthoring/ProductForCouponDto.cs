namespace Payment.Application.CouponAuthoring;

public sealed record ProductForCouponDto(Guid Id, Guid TourId, string Name, int PriceInEc);
