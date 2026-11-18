using Payment.Domain.Products;

namespace Payment.Application.Products;

public interface IProductRepository
{
    Task<Product?> GetByTourIdAsync(Guid tourId);
    void Add(Product product);
}
