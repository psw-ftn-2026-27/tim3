using Identity.Contracts;
using Payment.Application.GiftCards;
using Shared.Domain;

namespace Payment.Application.GiftCardReceiving;

public sealed class GiftCardReceivingQueries
{
    private readonly IGiftCardReadRepository _giftCardReadRepository;
    private readonly IIdentityApi _identityApi;

    public GiftCardReceivingQueries(IGiftCardReadRepository giftCardReadRepository, IIdentityApi identityApi)
    {
        _giftCardReadRepository = giftCardReadRepository;
        _identityApi = identityApi;
    }

    public async Task<PageResult<ReceivedGiftCardDto>> GetMyReceivedGiftCardsAsync(Guid recipientId, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var rows = await _giftCardReadRepository.GetReceivedByRecipientAsync(recipientId, DateTime.UtcNow, page, pageSize);
        var names = await _identityApi.GetDisplayNamesAsync(rows.Items.Select(row => row.BuyerId).Distinct().ToList());
        var namesById = names.ToDictionary(name => name.UserId, name => name.DisplayName);

        var items = rows.Items
            .Select(row => new ReceivedGiftCardDto(
                row.Id, row.BuyerId, namesById.GetValueOrDefault(row.BuyerId, ""),
                row.Amount, row.Message, row.ExpiresAt, row.Status))
            .ToList();
        return new PageResult<ReceivedGiftCardDto>(items, rows.TotalCount);
    }
}
