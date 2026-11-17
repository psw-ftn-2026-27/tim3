using Payment.Application.Products;
using Payment.Application.TourOffering;
using Payment.Contracts;

namespace Payment.Application;

public sealed class PaymentApi : IPaymentApi
{
    private readonly TourOfferingService _tourOfferingService;
    private readonly IProductReadRepository _productReadRepository;

    public PaymentApi(TourOfferingService tourOfferingService, IProductReadRepository productReadRepository)
    {
        _tourOfferingService = tourOfferingService;
        _productReadRepository = productReadRepository;
    }

    public Task CreateProductAsync(Guid tourId, string name, Guid authorId, int priceInEc)
    {
        return _tourOfferingService.CreateProductAsync(tourId, name, authorId, priceInEc);
    }

    public Task<Dictionary<Guid, int>> GetPricesByTourIdsAsync(List<Guid> tourIds)
    {
        return _productReadRepository.GetPricesByTourIdsAsync(tourIds);
    }
}
