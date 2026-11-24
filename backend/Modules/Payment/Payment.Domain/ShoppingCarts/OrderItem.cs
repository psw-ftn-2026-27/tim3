namespace Payment.Domain.ShoppingCarts;

public sealed record OrderItem
{
    public Guid ProductId { get; private init; }
    public Guid TourId { get; private init; }
    public string Name { get; private init; } = null!;
    public int PriceInEc { get; private init; }
    public Guid AuthorId { get; private init; }
    // Order of adding; decides which of two equally priced items gets a general coupon.
    public long Sequence { get; private init; }

    private OrderItem() { }

    internal OrderItem(Guid productId, Guid tourId, string name, int priceInEc, Guid authorId, long sequence)
    {
        ProductId = productId;
        TourId = tourId;
        Name = name;
        PriceInEc = priceInEc;
        AuthorId = authorId;
        Sequence = sequence;
    }
}
