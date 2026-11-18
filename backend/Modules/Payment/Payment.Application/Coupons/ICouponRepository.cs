using Payment.Domain.Coupons;

namespace Payment.Application.Coupons;

public interface ICouponRepository
{
    Task<Coupon?> GetByCodeAsync(CouponCode code);
    Task<List<Coupon>> GetByIdsAsync(IReadOnlyCollection<Guid> ids);
    void Add(Coupon coupon);
}
