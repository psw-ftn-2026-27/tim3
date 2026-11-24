using Payment.Application.Coupons;
using Payment.Application.Products;
using Payment.Domain.Coupons;

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
        var products = await _products.GetByAuthorIdAsync(authorId);
        var availableTourIds = await _availability.GetAvailableTourIdsAsync(products.Select(product => product.TourId));
        var request = new CouponRequest(dto.ProductId, dto.DiscountPercent, dto.ValidUntil);

        var coupon = _issuing.Issue(authorId, products, availableTourIds, request, DateTime.UtcNow);
        _coupons.Add(coupon);
        await _unitOfWork.SaveChangesAsync();
    }
}
