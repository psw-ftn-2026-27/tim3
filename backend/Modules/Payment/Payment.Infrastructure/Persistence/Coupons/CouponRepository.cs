using Microsoft.EntityFrameworkCore;
using Payment.Application.Coupons;
using Payment.Domain.Coupons;

namespace Payment.Infrastructure.Persistence.Coupons;

internal sealed class CouponRepository : ICouponRepository
{
    private readonly PaymentDbContext _dbContext;

    public CouponRepository(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Coupon?> GetByCodeAsync(CouponCode code)
    {
        return _dbContext.Coupons.SingleOrDefaultAsync(coupon => coupon.Code.Value == code.Value);
    }

    public Task<List<Coupon>> GetByIdsAsync(IReadOnlyCollection<Guid> ids)
    {
        return _dbContext.Coupons.Where(coupon => ids.Contains(coupon.Id)).ToListAsync();
    }

    public void Add(Coupon coupon)
    {
        _dbContext.Coupons.Add(coupon);
    }
}
