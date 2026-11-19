using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.CartGiftCard;
using Shared.Api;

namespace Payment.Api;

[ApiController]
[Route("api/payment/cart")]
[Authorize(Roles = "explorer")]
public sealed class CartGiftCardController : ControllerBase
{
    private readonly CartGiftCardQueries _queries;
    private readonly CartGiftCardService _service;

    public CartGiftCardController(CartGiftCardQueries queries, CartGiftCardService service)
    {
        _queries = queries;
        _service = service;
    }

    [HttpGet("gift-cards")]
    public async Task<ActionResult<List<GiftCardForCartDto>>> GetMyGiftCardsForCart()
    {
        return await _queries.GetMyGiftCardsForCartAsync(User.GetUserId());
    }

    [HttpPost("gift-card/{giftCardId:guid}")]
    public async Task<ActionResult<object>> EnterGiftCard(Guid giftCardId)
    {
        await _service.EnterGiftCardAsync(User.GetUserId(), giftCardId);
        return NoContent();
    }

    [HttpDelete("gift-card")]
    public async Task<ActionResult<object>> RemoveGiftCard()
    {
        await _service.RemoveGiftCardAsync(User.GetUserId());
        return NoContent();
    }
}
