using Payment.Application.Products;
using Shared.Domain.Exceptions;

namespace Payment.Application.ShoppingCart;

public sealed class ShoppingCartQueries
{
    private readonly IShoppingCartReadRepository _carts;
    private readonly IProductReadRepository _products;

    public ShoppingCartQueries(IShoppingCartReadRepository carts, IProductReadRepository products)
    {
        _carts = carts;
        _products = products;
    }

    public async Task<ShoppingCartDto> GetMyCartAsync(Guid userId)
    {
        return await _carts.GetByUserIdAsync(userId) ?? new ShoppingCartDto([], 0);
    }

    public async Task<ProductForCartDto> GetProductForCartAsync(Guid tourId)
    {
        return await _products.GetProductForCartAsync(tourId)
            ?? throw new NotFoundException("The tour has no product on offer.");
    }
}
