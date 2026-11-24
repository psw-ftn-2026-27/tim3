using Microsoft.EntityFrameworkCore;
using Payment.Application.CouponAuthoring;
using Payment.Application.Coupons;

namespace Payment.Infrastructure.Persistence.Coupons;

internal sealed class CouponReadRepository : ICouponReadRepository
{
    private readonly PaymentDbContext _dbContext;

    public CouponReadRepository(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<CouponDto>> GetByAuthorAsync(Guid authorId, DateOnly today)
    {
        // Mirrors Coupon.IsExpiredOn, so the list is projected without loading aggregates.
        return _dbContext.Coupons
            .Where(coupon => coupon.AuthorId == authorId)
            .OrderByDescending(coupon => coupon.CreatedAt)
            .ThenBy(coupon => coupon.Id)
            .Select(coupon => new CouponDto(
                coupon.Id,
                coupon.Code.Value,
                coupon.DiscountPercent,
                coupon.ValidUntil,
                coupon.ProductId,
                _dbContext.Products
                    .Where(product => product.Id == coupon.ProductId)
                    .Select(product => product.Name)
                    .FirstOrDefault(),
                today > coupon.ValidUntil))
            .ToListAsync();
    }
}
