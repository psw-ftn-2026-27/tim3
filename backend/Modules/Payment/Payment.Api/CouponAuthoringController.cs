using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.CouponAuthoring;
using Shared.Api;

namespace Payment.Api;

[ApiController]
[Route("api/payment/coupons")]
[Authorize(Roles = "explorer")]
public sealed class CouponAuthoringController : ControllerBase
{
    private readonly CouponAuthoringService _service;
    private readonly CouponAuthoringQueries _queries;

    public CouponAuthoringController(CouponAuthoringService service, CouponAuthoringQueries queries)
    {
        _service = service;
        _queries = queries;
    }

    [HttpGet]
    public async Task<ActionResult<List<CouponDto>>> GetMyCoupons()
    {
        return await _queries.GetMyCouponsAsync(User.GetUserId());
    }

    [HttpGet("products")]
    public async Task<ActionResult<List<ProductForCouponDto>>> GetMyProductsForCoupon()
    {
        return await _queries.GetMyProductsForCouponAsync(User.GetUserId());
    }

    [HttpPost]
    public async Task<ActionResult> CreateCoupon(CreateCouponDto dto)
    {
        await _service.CreateAsync(User.GetUserId(), dto);
        return NoContent();
    }
}
