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
    private readonly ShoppingCartCheckoutService _checkout;

    public ShoppingCartController(
        ShoppingCartQueries queries, ShoppingCartService service, ShoppingCartCheckoutService checkout)
    {
        _queries = queries;
        _service = service;
        _checkout = checkout;
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

    [HttpPost("coupons")]
    public async Task<ActionResult<object>> EnterCoupon(EnterCouponDto dto)
    {
        await _service.EnterCouponAsync(User.GetUserId(), dto);
        return NoContent();
    }

    [HttpDelete("coupons/{couponId:guid}")]
    public async Task<ActionResult<object>> RemoveCoupon(Guid couponId)
    {
        await _service.RemoveCouponAsync(User.GetUserId(), couponId);
        return NoContent();
    }

    [HttpPost("checkout")]
    public async Task<ActionResult<object>> Checkout(CheckoutDto dto)
    {
        await _checkout.CheckoutAsync(User.GetUserId(), dto);
        return NoContent();
    }
}
