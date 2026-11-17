namespace Payment.Contracts;

public interface IPaymentApi
{
    Task CreateProductAsync(Guid tourId, string name, Guid authorId, int priceInEc);

    // Missing products are omitted. A free tour has price 0.
    Task<Dictionary<Guid, int>> GetPricesByTourIdsAsync(List<Guid> tourIds);
}
