namespace Payment.Application.ShoppingCart;

public interface IShoppingCartReadRepository
{
    Task<ShoppingCartDto?> GetByUserIdAsync(Guid userId);
}
