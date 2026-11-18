using Payment.Application.GiftCardPurchasing;
using Payment.Application.GiftCardReceiving;
using Payment.Application.CartGiftCard;
using Shared.Domain;

namespace Payment.Application.GiftCards;

public interface IGiftCardReadRepository
{
    Task<PageResult<PurchasedGiftCardRowDto>> GetPurchasedByBuyerAsync(Guid buyerId, DateTime now, int page, int pageSize);
    Task<PageResult<ReceivedGiftCardRowDto>> GetReceivedByRecipientAsync(Guid recipientId, DateTime now, int page, int pageSize);
    Task<List<GiftCardForCartDto>> GetUsableByRecipientAsync(Guid recipientId, DateTime now);
}
