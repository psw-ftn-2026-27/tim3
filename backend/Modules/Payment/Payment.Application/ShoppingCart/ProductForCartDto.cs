namespace Payment.Application.ShoppingCart;

public sealed record ProductForCartDto(Guid TourId, string Name, int PriceInEc);
