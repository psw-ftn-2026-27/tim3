using Payment.Application.GiftCardPurchasing;

namespace Payment.Application.GiftCards;

public interface IGiftCardReadRepository
{
    Task<List<PurchasedGiftCardRowDto>> GetPurchasedByBuyerAsync(Guid buyerId, DateTime now);
}
