using Microsoft.EntityFrameworkCore;
using Payment.Application.GiftCardPurchasing;
using Payment.Application.GiftCards;
using Payment.Domain.GiftCards;
using Shared.Domain;

namespace Payment.Infrastructure.Persistence.GiftCards;

internal sealed class GiftCardReadRepository : IGiftCardReadRepository
{
    private readonly PaymentDbContext _dbContext;

    public GiftCardReadRepository(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PageResult<PurchasedGiftCardRowDto>> GetPurchasedByBuyerAsync(
        Guid buyerId, DateTime now, int page, int pageSize)
    {
        var purchased = _dbContext.GiftCards.Where(giftCard => giftCard.BuyerId == buyerId);
        var totalCount = await purchased.CountAsync();
        var offset = (int)Math.Min((page - 1L) * pageSize, int.MaxValue);

        // Mirrors GiftCard.StatusAt, so the list is projected without loading aggregates.
        var items = await purchased
            .OrderByDescending(giftCard => giftCard.PurchasedAt)
            .ThenBy(giftCard => giftCard.Id)
            .Skip(offset)
            .Take(pageSize)
            .Select(giftCard => new PurchasedGiftCardRowDto(
                giftCard.Id,
                giftCard.RecipientId,
                giftCard.Amount,
                giftCard.Message,
                giftCard.ExpiresAt,
                now < giftCard.ExpiresAt ? GiftCardStatus.Valid : GiftCardStatus.Expired))
            .ToListAsync();
        return new PageResult<PurchasedGiftCardRowDto>(items, totalCount);
    }
}
