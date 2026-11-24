using Payment.Domain.Products;

namespace Payment.Application.Products;

public interface IProductRepository
{
    Task<Product?> GetByTourIdAsync(Guid tourId);
    Task<List<Product>> GetByAuthorIdAsync(Guid authorId);
    void Add(Product product);
}
