using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Payment.Domain.Coupons;

public sealed class Coupon : AggregateRoot
{
    private const int MinDiscountPercent = 1;
    private const int MaxDiscountPercent = 100;

    public Guid AuthorId { get; private set; }
    public Guid? ProductId { get; private set; }
    public CouponCode Code { get; private set; } = null!;
    public int DiscountPercent { get; private set; }
    public DateOnly ValidUntil { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Coupon() { }

    public Coupon(Guid authorId, CouponRequest request, CouponCode code, DateTime createdAt)
        : base(Guid.NewGuid())
    {
        if (authorId == Guid.Empty)
        {
            throw new DomainException("A coupon must have an author.");
        }

        if (request.DiscountPercent < MinDiscountPercent || request.DiscountPercent > MaxDiscountPercent)
        {
            throw new DomainException(
                $"The coupon discount must be a whole percent between {MinDiscountPercent} and {MaxDiscountPercent}.");
        }

        if (request.ValidUntil < DateOnly.FromDateTime(createdAt))
        {
            throw new DomainException("A coupon cannot be valid only until a day before today.");
        }

        AuthorId = authorId;
        ProductId = request.ProductId;
        Code = code ?? throw new DomainException("A coupon requires a code.");
        DiscountPercent = request.DiscountPercent;
        ValidUntil = request.ValidUntil;
        CreatedAt = createdAt;
    }

    public bool IsExpiredOn(DateOnly date)
    {
        return date > ValidUntil;
    }
}
