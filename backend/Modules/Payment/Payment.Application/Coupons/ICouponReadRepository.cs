using Payment.Application.CouponAuthoring;

namespace Payment.Application.Coupons;

public interface ICouponReadRepository
{
    Task<List<CouponDto>> GetByAuthorAsync(Guid authorId, DateOnly today);
}
