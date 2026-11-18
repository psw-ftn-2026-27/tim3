namespace Payment.Domain.ShoppingCarts;

// What the buyer has already done, read from their purchase tokens: the tours they own and the coupons they used.
public sealed record BuyerHistory(IReadOnlySet<Guid> PurchasedTourIds, IReadOnlySet<Guid> UsedCouponIds);
