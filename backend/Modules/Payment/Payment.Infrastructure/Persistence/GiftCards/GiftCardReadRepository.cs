using Microsoft.EntityFrameworkCore;
using Payment.Application.GiftCardPurchasing;
using Payment.Application.GiftCards;
using Payment.Domain.GiftCards;

namespace Payment.Infrastructure.Persistence.GiftCards;

internal sealed class GiftCardReadRepository : IGiftCardReadRepository
{
    private readonly PaymentDbContext _dbContext;

    public GiftCardReadRepository(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<PurchasedGiftCardRowDto>> GetPurchasedByBuyerAsync(Guid buyerId, DateTime now)
    {
        // Mirrors GiftCard.StatusAt, so the list is projected without loading aggregates.
        return _dbContext.GiftCards
            .Where(giftCard => giftCard.BuyerId == buyerId)
            .OrderByDescending(giftCard => giftCard.PurchasedAt)
            .ThenBy(giftCard => giftCard.Id)
            .Select(giftCard => new PurchasedGiftCardRowDto(
                giftCard.Id,
                giftCard.RecipientId,
                giftCard.Amount,
                giftCard.Message,
                giftCard.ExpiresAt,
                now < giftCard.ExpiresAt ? GiftCardStatus.Valid : GiftCardStatus.Expired))
            .ToListAsync();
    }
}
