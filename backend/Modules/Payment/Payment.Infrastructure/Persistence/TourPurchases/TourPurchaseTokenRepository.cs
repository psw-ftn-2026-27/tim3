using Microsoft.EntityFrameworkCore;
using Payment.Application.ShoppingCart;
using Payment.Domain.TourPurchases;

namespace Payment.Infrastructure.Persistence.TourPurchases;

internal sealed class TourPurchaseTokenRepository : ITourPurchaseTokenRepository
{
    private readonly PaymentDbContext _dbContext;

    public TourPurchaseTokenRepository(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> ExistsAsync(Guid buyerId, Guid tourId)
    {
        return _dbContext.TourPurchaseTokens.AnyAsync(token => token.BuyerId == buyerId && token.TourId == tourId);
    }

    public Task<List<Guid>> GetPurchasedTourIdsAsync(Guid buyerId)
    {
        return _dbContext.TourPurchaseTokens
            .Where(token => token.BuyerId == buyerId)
            .Select(token => token.TourId)
            .ToListAsync();
    }

    public Task<List<Guid>> GetUsedCouponIdsAsync(Guid buyerId)
    {
        return _dbContext.TourPurchaseTokens
            .Where(token => token.BuyerId == buyerId && token.CouponId != null)
            .Select(token => token.CouponId!.Value)
            .ToListAsync();
    }

    public void Add(TourPurchaseToken token)
    {
        _dbContext.TourPurchaseTokens.Add(token);
    }
}
