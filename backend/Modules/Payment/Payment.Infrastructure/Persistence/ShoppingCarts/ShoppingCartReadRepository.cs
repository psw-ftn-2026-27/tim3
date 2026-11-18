using Microsoft.EntityFrameworkCore;
using Payment.Application.ShoppingCart;

namespace Payment.Infrastructure.Persistence.ShoppingCarts;

internal sealed class ShoppingCartReadRepository : IShoppingCartReadRepository
{
    private readonly PaymentDbContext _dbContext;

    public ShoppingCartReadRepository(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ShoppingCartDto?> GetByUserIdAsync(Guid userId)
    {
        return _dbContext.ShoppingCarts
            .Where(cart => cart.UserId == userId)
            .Select(cart => new ShoppingCartDto(
                cart.Items.Select(item => new OrderItemDto(item.ProductId, item.TourId, item.Name, item.PriceInEc))
                    .ToList(),
                cart.Items.Sum(item => (long)item.PriceInEc)))
            .SingleOrDefaultAsync();
    }
}
