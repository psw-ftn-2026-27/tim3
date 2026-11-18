namespace Payment.Domain.ShoppingCarts;

public sealed record OrderItem
{
    public Guid ProductId { get; private init; }
    public Guid TourId { get; private init; }
    public string Name { get; private init; } = null!;
    public int PriceInEc { get; private init; }

    private OrderItem() { }

    internal OrderItem(Guid productId, Guid tourId, string name, int priceInEc)
    {
        ProductId = productId;
        TourId = tourId;
        Name = name;
        PriceInEc = priceInEc;
    }
}
