using Payment.Application.Coupons;
using Payment.Application.Products;
using Payment.Domain.Coupons;
using Payment.Domain.Products;

namespace Payment.Application.CouponAuthoring;

public sealed class CouponAuthoringService
{
    private readonly ICouponRepository _coupons;
    private readonly IProductRepository _products;
    private readonly ITourAvailability _availability;
    private readonly CouponIssuing _issuing;
    private readonly IUnitOfWork _unitOfWork;

    public CouponAuthoringService(
        ICouponRepository coupons,
        IProductRepository products,
        ITourAvailability availability,
        CouponIssuing issuing,
        IUnitOfWork unitOfWork)
    {
        _coupons = coupons;
        _products = products;
        _availability = availability;
        _issuing = issuing;
        _unitOfWork = unitOfWork;
    }

    public async Task CreateAsync(Guid authorId, CreateCouponDto dto)
    {
        var now = DateTime.UtcNow;
        var products = await _products.GetByAuthorIdAsync(authorId);
        var availableTourIds = await GetAvailableTourIdsAsync(products);

        var coupon = _issuing.Issue(
            authorId, products, availableTourIds, dto.ProductId, dto.DiscountPercent, dto.ValidUntil,
            DateOnly.FromDateTime(now), CouponCode.Generate(), now);
        _coupons.Add(coupon);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<HashSet<Guid>> GetAvailableTourIdsAsync(List<Product> products)
    {
        var available = new HashSet<Guid>();
        foreach (var product in products)
        {
            if (await _availability.IsAvailableAsync(product.TourId))
            {
                available.Add(product.TourId);
            }
        }
        return available;
    }
}
