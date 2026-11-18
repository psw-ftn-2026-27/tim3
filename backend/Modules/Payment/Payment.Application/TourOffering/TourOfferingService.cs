using Payment.Application.Products;
using Payment.Domain.Products;

namespace Payment.Application.TourOffering;

public sealed class TourOfferingService
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TourOfferingService(IProductRepository productRepository, IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task CreateProductAsync(Guid tourId, string name, Guid authorId, int priceInEc)
    {
        var existingProduct = await _productRepository.GetByTourIdAsync(tourId);
        if (existingProduct is not null)
        {
            existingProduct.EnsureSameOffer(name, authorId, priceInEc);
            return;
        }

        _productRepository.Add(new Product(tourId, name, authorId, priceInEc));
        await _unitOfWork.SaveChangesAsync();
    }
}
