namespace Payment.Domain.Wallets;

// The charge for one tour: its price in the cart and the coupon discount taken off it.
public sealed record TourCharge(Guid TourId, int PriceInEc, int DiscountInEc);
