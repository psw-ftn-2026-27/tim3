using Payment.Domain.Products;
using Shared.Domain.Exceptions;

namespace Payment.Domain.Coupons;

public sealed class CouponIssuing
{
    public Coupon Issue(
        Guid authorId,
        IReadOnlyCollection<Product> authorProducts,
        IReadOnlySet<Guid> availableTourIds,
        Guid? productId,
        int discountPercent,
        DateOnly validUntil,
        DateOnly today,
        CouponCode code,
        DateTime createdAt)
    {
        var ownProducts = authorProducts.Where(product => product.AuthorId == authorId).ToList();
        if (productId is null)
        {
            if (!ownProducts.Any(product => IsPaidAndOnOffer(product, availableTourIds)))
            {
                throw new DomainException("A coupon requires at least one paid product whose tour is on offer.");
            }
        }
        else
        {
            var product = ownProducts.SingleOrDefault(product => product.Id == productId)
                ?? throw new NotFoundException($"Product {productId} does not exist among your products.");
            if (product.PriceInEc == 0)
            {
                throw new DomainException("A coupon cannot be tied to a free product.");
            }

            if (!availableTourIds.Contains(product.TourId))
            {
                throw new DomainException("A coupon cannot be tied to a tour that is not on offer.");
            }
        }

        return new Coupon(authorId, productId, code, discountPercent, validUntil, today, createdAt);
    }

    private static bool IsPaidAndOnOffer(Product product, IReadOnlySet<Guid> availableTourIds)
    {
        return product.PriceInEc > 0 && availableTourIds.Contains(product.TourId);
    }
}
