using Microsoft.EntityFrameworkCore;
using Payment.Application.CouponAuthoring;
using Payment.Application.Products;
using Payment.Application.ShoppingCart;

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

    public Task<ProductForCartDto?> GetProductForCartAsync(Guid tourId)
    {
        return _dbContext.Products
            .Where(product => product.TourId == tourId)
            .Select(product => new ProductForCartDto(product.TourId, product.Name, product.PriceInEc))
            .SingleOrDefaultAsync();
    }

    public Task<List<ProductForCouponDto>> GetPaidByAuthorAsync(Guid authorId)
    {
        return _dbContext.Products
            .Where(product => product.AuthorId == authorId && product.PriceInEc > 0)
            .OrderBy(product => product.Name)
            .Select(product => new ProductForCouponDto(product.Id, product.TourId, product.Name, product.PriceInEc))
            .ToListAsync();
    }
}
