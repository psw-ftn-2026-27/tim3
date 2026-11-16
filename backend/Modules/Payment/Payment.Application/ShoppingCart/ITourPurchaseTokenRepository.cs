using Payment.Domain.TourPurchases;

namespace Payment.Application.ShoppingCart;

public interface ITourPurchaseTokenRepository
{
    Task<bool> ExistsAsync(Guid buyerId, Guid tourId);
    void Add(TourPurchaseToken token);
}
