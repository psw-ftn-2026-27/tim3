using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.GiftCardPurchasing;
using Shared.Api;
using Shared.Domain;

namespace Payment.Api;

[ApiController]
[Route("api/payment/gift-cards")]
[Authorize(Roles = "explorer")]
public sealed class GiftCardPurchasingController : ControllerBase
{
    private readonly GiftCardPurchasingService _purchasingService;
    private readonly GiftCardPurchasingQueries _purchasingQueries;

    public GiftCardPurchasingController(
        GiftCardPurchasingService purchasingService,
        GiftCardPurchasingQueries purchasingQueries)
    {
        _purchasingService = purchasingService;
        _purchasingQueries = purchasingQueries;
    }

    [HttpGet("purchased")]
    public async Task<ActionResult<PageResult<PurchasedGiftCardDto>>> GetMyPurchasedGiftCards(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return await _purchasingQueries.GetMyPurchasedGiftCardsAsync(User.GetUserId(), page, pageSize);
    }

    [HttpGet("recipients")]
    public async Task<ActionResult<PageResult<RecipientDto>>> SearchRecipients(
        [FromQuery] string? text = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return await _purchasingQueries.SearchRecipientsAsync(User.GetUserId(), text ?? string.Empty, page, pageSize);
    }

    [HttpPost]
    public async Task<ActionResult> BuyGiftCard(BuyGiftCardDto dto)
    {
        await _purchasingService.BuyAsync(User.GetUserId(), dto);
        return NoContent();
    }
}
