using Microsoft.EntityFrameworkCore;
using Payment.Application.Products;

namespace Payment.Infrastructure.Persistence.Products;

internal sealed class ProductReadRepository : IProductReadRepository
{
    private readonly PaymentDbContext _dbContext;

    public ProductReadRepository(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Dictionary<Guid, int>> GetPricesByTourIdsAsync(List<Guid> tourIds)
    {
        return _dbContext.Products
            .Where(product => tourIds.Contains(product.TourId))
            .Select(product => new { product.TourId, product.PriceInEc })
            .ToDictionaryAsync(product => product.TourId, product => product.PriceInEc);
    }
}
