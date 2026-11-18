namespace Payment.Application.CartGiftCard;

public sealed record GiftCardForCartDto(Guid Id, int Amount, DateTime ExpiresAt);
