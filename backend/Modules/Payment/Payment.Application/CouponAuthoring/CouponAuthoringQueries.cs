using Payment.Application.Coupons;
using Payment.Application.Products;

namespace Payment.Application.CouponAuthoring;

public sealed class CouponAuthoringQueries
{
    private readonly ICouponReadRepository _coupons;
    private readonly IProductReadRepository _products;
    private readonly ITourAvailability _availability;

    public CouponAuthoringQueries(
        ICouponReadRepository coupons, IProductReadRepository products, ITourAvailability availability)
    {
        _coupons = coupons;
        _products = products;
        _availability = availability;
    }

    public Task<List<CouponDto>> GetMyCouponsAsync(Guid authorId)
    {
        return _coupons.GetByAuthorAsync(authorId, DateOnly.FromDateTime(DateTime.UtcNow));
    }

    public async Task<List<ProductForCouponDto>> GetMyProductsForCouponAsync(Guid authorId)
    {
        var paidProducts = await _products.GetPaidByAuthorAsync(authorId);
        var productsOnOffer = new List<ProductForCouponDto>();
        foreach (var product in paidProducts)
        {
            if (await _availability.IsAvailableAsync(product.TourId))
            {
                productsOnOffer.Add(product);
            }
        }
        return productsOnOffer;
    }
}
