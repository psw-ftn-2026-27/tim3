using Payment.Domain.GiftCards;

namespace Payment.Application.GiftCardReceiving;

public sealed record ReceivedGiftCardDto(
    Guid Id,
    Guid BuyerId,
    string BuyerDisplayName,
    int Amount,
    string? Message,
    DateTime ExpiresAt,
    GiftCardStatus Status);
