namespace Payment.Application.Products;

public interface ITourAvailability
{
    Task<bool> IsAvailableAsync(Guid tourId);
}
