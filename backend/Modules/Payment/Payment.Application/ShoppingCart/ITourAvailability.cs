namespace Payment.Application.ShoppingCart;

public interface ITourAvailability
{
    Task<bool> IsAvailableAsync(Guid tourId);
}
