using Payment.Domain.TourPurchases;

namespace Payment.Application.ShoppingCart;

public interface ITourPurchaseTokenRepository
{
    Task<List<Guid>> GetPurchasedTourIdsAsync(Guid buyerId);
    Task<List<Guid>> GetUsedCouponIdsAsync(Guid buyerId);
    Task<bool> ExistsAsync(Guid buyerId, Guid tourId);
    void Add(TourPurchaseToken token);
}
