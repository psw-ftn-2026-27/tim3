using Payment.Domain.GiftCards;

namespace Payment.Application.GiftCardPurchasing;

public sealed record PurchasedGiftCardDto(
    Guid Id,
    Guid RecipientId,
    string RecipientDisplayName,
    int Amount,
    string? Message,
    DateTime ExpiresAt,
    GiftCardStatus Status);
