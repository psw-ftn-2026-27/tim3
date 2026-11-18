using Cart = Payment.Domain.ShoppingCarts.ShoppingCart;

namespace Payment.Application.ShoppingCart;

public interface IShoppingCartRepository
{
    Task<Cart?> GetByUserIdAsync(Guid userId);
    void Add(Cart cart);
}
