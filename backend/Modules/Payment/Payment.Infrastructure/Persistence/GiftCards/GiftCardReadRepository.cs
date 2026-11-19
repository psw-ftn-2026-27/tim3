using Microsoft.EntityFrameworkCore;
using Payment.Application.CartGiftCard;
using Payment.Application.GiftCardPurchasing;
using Payment.Application.GiftCardReceiving;
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
                giftCard.RedeemedAt != null
                    ? GiftCardStatus.Redeemed
                    : now < giftCard.ExpiresAt ? GiftCardStatus.Valid : GiftCardStatus.Expired))
            .ToListAsync();
        return new PageResult<PurchasedGiftCardRowDto>(items, totalCount);
    }

    public async Task<PageResult<ReceivedGiftCardRowDto>> GetReceivedByRecipientAsync(
        Guid recipientId, DateTime now, int page, int pageSize)
    {
        var received = _dbContext.GiftCards.Where(giftCard => giftCard.RecipientId == recipientId);
        var totalCount = await received.CountAsync();
        var offset = (int)Math.Min((page - 1L) * pageSize, int.MaxValue);

        // Mirrors GiftCard.StatusAt, so the list is projected without loading aggregates.
        var items = await received
            .OrderByDescending(giftCard => giftCard.PurchasedAt)
            .ThenBy(giftCard => giftCard.Id)
            .Skip(offset)
            .Take(pageSize)
            .Select(giftCard => new ReceivedGiftCardRowDto(
                giftCard.Id,
                giftCard.BuyerId,
                giftCard.Amount,
                giftCard.Message,
                giftCard.ExpiresAt,
                giftCard.RedeemedAt != null
                    ? GiftCardStatus.Redeemed
                    : now < giftCard.ExpiresAt ? GiftCardStatus.Valid : GiftCardStatus.Expired))
            .ToListAsync();
        return new PageResult<ReceivedGiftCardRowDto>(items, totalCount);
    }

    // Mirrors GiftCard.StatusAt: only cards that are neither redeemed nor expired can be chosen in the cart.
    public Task<List<GiftCardForCartDto>> GetUsableByRecipientAsync(Guid recipientId, DateTime now)
    {
        return _dbContext.GiftCards
            .Where(giftCard => giftCard.RecipientId == recipientId
                && giftCard.RedeemedAt == null
                && now < giftCard.ExpiresAt)
            .OrderBy(giftCard => giftCard.ExpiresAt)
            .ThenBy(giftCard => giftCard.Id)
            .Select(giftCard => new GiftCardForCartDto(giftCard.Id, giftCard.Amount, giftCard.ExpiresAt))
            .ToListAsync();
    }
}
