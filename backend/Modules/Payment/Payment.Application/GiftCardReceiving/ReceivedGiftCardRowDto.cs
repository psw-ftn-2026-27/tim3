using Payment.Domain.GiftCards;

namespace Payment.Application.GiftCardReceiving;

public sealed record ReceivedGiftCardRowDto(
    Guid Id, Guid BuyerId, int Amount, string? Message, DateTime ExpiresAt, GiftCardStatus Status);
