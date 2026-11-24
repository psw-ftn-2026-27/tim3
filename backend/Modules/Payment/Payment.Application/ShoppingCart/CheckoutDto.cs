namespace Payment.Application.ShoppingCart;

// The total to pay from the wallet the buyer saw when confirming; the purchase is refused when it no longer matches.
public sealed record CheckoutDto(long SeenTotalInEc);
