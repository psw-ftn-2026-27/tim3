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

    public Coupon(
        Guid authorId,
        Guid? productId,
        CouponCode code,
        int discountPercent,
        DateOnly validUntil,
        DateOnly today,
        DateTime createdAt)
        : base(Guid.NewGuid())
    {
        if (authorId == Guid.Empty)
        {
            throw new DomainException("A coupon must have an author.");
        }

        if (discountPercent < MinDiscountPercent || discountPercent > MaxDiscountPercent)
        {
            throw new DomainException(
                $"The coupon discount must be a whole percent between {MinDiscountPercent} and {MaxDiscountPercent}.");
        }

        if (validUntil < today)
        {
            throw new DomainException("A coupon cannot be valid only until a day before today.");
        }

        AuthorId = authorId;
        ProductId = productId;
        Code = code ?? throw new DomainException("A coupon requires a code.");
        DiscountPercent = discountPercent;
        ValidUntil = validUntil;
        CreatedAt = createdAt;
    }

    public bool IsExpiredOn(DateOnly date)
    {
        return date > ValidUntil;
    }
}
