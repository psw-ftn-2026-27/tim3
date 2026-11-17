using Payment.Application.Products;
using Payment.Application.ShoppingCart;
using Payment.Application.TourOffering;
using Payment.Contracts;

namespace Payment.Application;

public sealed class PaymentApi : IPaymentApi
{
    private readonly TourOfferingService _tourOfferingService;
    private readonly IProductReadRepository _productReadRepository;
    private readonly ITourPurchaseTokenRepository _purchaseTokens;

    public PaymentApi(
        TourOfferingService tourOfferingService,
        IProductReadRepository productReadRepository,
        ITourPurchaseTokenRepository purchaseTokens)
    {
        _tourOfferingService = tourOfferingService;
        _productReadRepository = productReadRepository;
        _purchaseTokens = purchaseTokens;
    }

    public Task CreateProductAsync(Guid tourId, string name, Guid authorId, int priceInEc)
    {
        return _tourOfferingService.CreateProductAsync(tourId, name, authorId, priceInEc);
    }

    public Task<Dictionary<Guid, int>> GetPricesByTourIdsAsync(List<Guid> tourIds)
    {
        return _productReadRepository.GetPricesByTourIdsAsync(tourIds);
    }

    public Task<List<Guid>> GetPurchasedTourIdsAsync(Guid buyerId)
    {
        return _purchaseTokens.GetTourIdsByBuyerAsync(buyerId);
    }

    public Task<bool> OwnsTourAsync(Guid buyerId, Guid tourId)
    {
        return _purchaseTokens.ExistsAsync(buyerId, tourId);
    }
}
