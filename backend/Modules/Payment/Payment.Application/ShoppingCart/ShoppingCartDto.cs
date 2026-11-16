namespace Payment.Application.ShoppingCart;

public sealed record OrderItemDto(Guid ProductId, Guid TourId, string Name, int PriceInEc);

public sealed record ShoppingCartDto(List<OrderItemDto> Items, long TotalInEc);
