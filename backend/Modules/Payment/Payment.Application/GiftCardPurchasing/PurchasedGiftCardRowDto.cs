using Payment.Domain.GiftCards;

namespace Payment.Application.GiftCardPurchasing;

public sealed record PurchasedGiftCardRowDto(
    Guid Id, Guid RecipientId, int Amount, string? Message, DateTime ExpiresAt, GiftCardStatus Status);
