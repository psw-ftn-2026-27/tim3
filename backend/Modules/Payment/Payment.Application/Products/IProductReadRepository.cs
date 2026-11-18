using Payment.Application.ShoppingCart;

namespace Payment.Application.Products;

public interface IProductReadRepository
{
    Task<Dictionary<Guid, int>> GetPricesByTourIdsAsync(List<Guid> tourIds);
    Task<ProductForCartDto?> GetProductForCartAsync(Guid tourId);
}
