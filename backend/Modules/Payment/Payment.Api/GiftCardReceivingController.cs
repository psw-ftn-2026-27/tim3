using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.GiftCardReceiving;
using Shared.Api;
using Shared.Domain;

namespace Payment.Api;

[ApiController]
[Route("api/payment/gift-cards")]
[Authorize(Roles = "explorer")]
public sealed class GiftCardReceivingController : ControllerBase
{
    private readonly GiftCardReceivingQueries _receivingQueries;

    public GiftCardReceivingController(GiftCardReceivingQueries receivingQueries)
    {
        _receivingQueries = receivingQueries;
    }

    [HttpGet("received")]
    public async Task<ActionResult<PageResult<ReceivedGiftCardDto>>> GetMyReceivedGiftCards(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return await _receivingQueries.GetMyReceivedGiftCardsAsync(User.GetUserId(), page, pageSize);
    }
}
