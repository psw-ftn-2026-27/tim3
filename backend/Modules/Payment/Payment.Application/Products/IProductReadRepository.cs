namespace Payment.Application.Products;

public interface IProductReadRepository
{
    Task<Dictionary<Guid, int>> GetPricesByTourIdsAsync(List<Guid> tourIds);
}
