using Microsoft.EntityFrameworkCore;
using Payment.Application.ShoppingCart;
using Payment.Domain.ShoppingCarts;

namespace Payment.Infrastructure.Persistence.ShoppingCarts;

internal sealed class ShoppingCartRepository : IShoppingCartRepository
{
    private readonly PaymentDbContext _dbContext;

    public ShoppingCartRepository(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ShoppingCart?> GetByUserIdAsync(Guid userId)
    {
        return _dbContext.ShoppingCarts.SingleOrDefaultAsync(cart => cart.UserId == userId);
    }

    public void Add(ShoppingCart cart)
    {
        _dbContext.ShoppingCarts.Add(cart);
    }
}
