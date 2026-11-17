using Payment.Application.GiftCardPurchasing;
using Shared.Domain;

namespace Payment.Application.GiftCards;

public interface IGiftCardReadRepository
{
    Task<PageResult<PurchasedGiftCardRowDto>> GetPurchasedByBuyerAsync(Guid buyerId, DateTime now, int page, int pageSize);
}
