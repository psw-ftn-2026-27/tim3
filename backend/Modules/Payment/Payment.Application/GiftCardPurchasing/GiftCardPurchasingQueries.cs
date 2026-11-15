using Identity.Contracts;
using Payment.Application.GiftCards;
using Shared.Domain;

namespace Payment.Application.GiftCardPurchasing;

public sealed class GiftCardPurchasingQueries
{
    private readonly IGiftCardReadRepository _giftCardReadRepository;
    private readonly IIdentityApi _identityApi;

    public GiftCardPurchasingQueries(IGiftCardReadRepository giftCardReadRepository, IIdentityApi identityApi)
    {
        _giftCardReadRepository = giftCardReadRepository;
        _identityApi = identityApi;
    }

    public async Task<List<PurchasedGiftCardDto>> GetMyPurchasedGiftCardsAsync(Guid buyerId)
    {
        var rows = await _giftCardReadRepository.GetPurchasedByBuyerAsync(buyerId, DateTime.UtcNow);
        var names = await _identityApi.GetDisplayNamesAsync(rows.Select(row => row.RecipientId).Distinct().ToList());
        var namesById = names.ToDictionary(name => name.UserId, name => name.DisplayName);

        return rows
            .Select(row => new PurchasedGiftCardDto(
                row.Id, row.RecipientId, namesById.GetValueOrDefault(row.RecipientId, ""),
                row.Amount, row.Message, row.ExpiresAt, row.Status))
            .ToList();
    }

    public async Task<PageResult<RecipientDto>> SearchRecipientsAsync(Guid buyerId, string text, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var users = await _identityApi.SearchUsersAsync(text, buyerId, page, pageSize);
        var items = users.Items.Select(user => new RecipientDto(user.UserId, user.DisplayName)).ToList();
        return new PageResult<RecipientDto>(items, users.TotalCount);
    }
}
