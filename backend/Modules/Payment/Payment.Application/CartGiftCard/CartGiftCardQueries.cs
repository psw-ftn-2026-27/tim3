using Payment.Application.GiftCards;

namespace Payment.Application.CartGiftCard;

public sealed class CartGiftCardQueries
{
    private readonly IGiftCardReadRepository _giftCards;

    public CartGiftCardQueries(IGiftCardReadRepository giftCards)
    {
        _giftCards = giftCards;
    }

    public async Task<List<GiftCardForCartDto>> GetMyGiftCardsForCartAsync(Guid userId)
    {
        return await _giftCards.GetUsableByRecipientAsync(userId, DateTime.UtcNow);
    }
}
