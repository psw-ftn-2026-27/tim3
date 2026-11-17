namespace Payment.Application.GiftCardPurchasing;

public sealed record BuyGiftCardDto(Guid RecipientId, int Amount, string? Message);
