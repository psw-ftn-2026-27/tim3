namespace Payment.Application.ShoppingCart;

public sealed record GiftCardForCartDto(Guid Id, int Amount, DateTime ExpiresAt);
