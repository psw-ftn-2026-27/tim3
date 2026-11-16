using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.ShoppingCart;
using Shared.Api;

namespace Payment.Api;

[ApiController]
[Route("api/payment/cart")]
[Authorize(Roles = "explorer")]
public sealed class ShoppingCartController : ControllerBase
{
    private readonly ShoppingCartQueries _queries;
    private readonly ShoppingCartService _service;

    public ShoppingCartController(ShoppingCartQueries queries, ShoppingCartService service)
    {
        _queries = queries;
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<ShoppingCartDto>> GetMyCart()
    {
        return await _queries.GetMyCartAsync(User.GetUserId());
    }

    [HttpGet("products/{tourId:guid}")]
    public async Task<ActionResult<ProductForCartDto>> GetProductForCart(Guid tourId)
    {
        return await _queries.GetProductForCartAsync(tourId);
    }

    [HttpPost("items/{tourId:guid}")]
    public async Task<ActionResult<object>> AddToCart(Guid tourId)
    {
        await _service.AddToCartAsync(User.GetUserId(), tourId);
        return NoContent();
    }

    [HttpDelete("items/{tourId:guid}")]
    public async Task<ActionResult<object>> RemoveFromCart(Guid tourId)
    {
        await _service.RemoveFromCartAsync(User.GetUserId(), tourId);
        return NoContent();
    }
}
