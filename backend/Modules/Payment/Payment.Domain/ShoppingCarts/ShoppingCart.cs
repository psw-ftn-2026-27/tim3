using Payment.Domain.Products;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Payment.Domain.ShoppingCarts;

public sealed class ShoppingCart : AggregateRoot
{
    private readonly List<OrderItem> _items = [];
    private readonly List<Guid> _couponIds = [];
    // Checkout must detect edits to the cart even when only its items, coupons or gift card changed.
    private long _revision;

    public Guid UserId { get; private set; }
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    public IReadOnlyList<Guid> CouponIds => _couponIds.AsReadOnly();
    public Guid? GiftCardId { get; private set; }
    public long TotalInEc => _items.Sum(item => (long)item.PriceInEc);

    private ShoppingCart() { }

    public ShoppingCart(Guid userId) : base(Guid.NewGuid())
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException("A cart must have an owner.");
        }

        UserId = userId;
    }

    public void AddItem(Product product)
    {
        if (_items.Any(item => item.TourId == product.TourId))
        {
            throw new DomainException("The tour is already in the cart.");
        }

        _revision++;
        _items.Add(new OrderItem(
            product.Id, product.TourId, product.Name, product.PriceInEc, product.AuthorId, _revision));
    }

    public void RemoveItem(Guid tourId)
    {
        var item = _items.SingleOrDefault(item => item.TourId == tourId)
            ?? throw new NotFoundException("The tour is not in the cart.");
        _items.Remove(item);
        _revision++;
    }

    internal void EnterCoupon(Guid couponId)
    {
        if (_couponIds.Contains(couponId))
        {
            throw new DomainException("The coupon is already in the cart.");
        }

        _couponIds.Add(couponId);
        _revision++;
    }

    public void RemoveCoupon(Guid couponId)
    {
        if (!_couponIds.Remove(couponId))
        {
            throw new NotFoundException("The coupon is not in the cart.");
        }

        _revision++;
    }

    // At most one gift card per purchase; another one is chosen only after the first is removed.
    internal void EnterGiftCard(Guid giftCardId)
    {
        if (GiftCardId is not null)
        {
            throw new DomainException("A gift card is already in the cart. Remove it first.");
        }

        GiftCardId = giftCardId;
        _revision++;
    }

    public void RemoveGiftCard()
    {
        if (GiftCardId is null)
        {
            throw new NotFoundException("The cart has no gift card.");
        }

        GiftCardId = null;
        _revision++;
    }

    internal void ClearAfterPurchase()
    {
        _items.Clear();
        _couponIds.Clear();
        GiftCardId = null;
        _revision++;
    }
}
