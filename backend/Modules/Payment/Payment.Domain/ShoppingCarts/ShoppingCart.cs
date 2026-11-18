using Payment.Domain.Products;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Payment.Domain.ShoppingCarts;

public sealed class ShoppingCart : AggregateRoot
{
    private readonly List<OrderItem> _items = [];
    // Checkout must detect edits to the cart even when only its items changed.
    private long _revision;

    public Guid UserId { get; private set; }
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
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

        _items.Add(new OrderItem(product.Id, product.TourId, product.Name, product.PriceInEc));
        _revision++;
    }

    public void RemoveItem(Guid tourId)
    {
        var item = _items.SingleOrDefault(item => item.TourId == tourId)
            ?? throw new NotFoundException("The tour is not in the cart.");
        _items.Remove(item);
        _revision++;
    }

    internal void ClearAfterPurchase()
    {
        _items.Clear();
        _revision++;
    }
}
