using Exploration.Application.TourBrowsing;
using Exploration.Application.Tours;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;
using Shared.Domain;

namespace Exploration.Api;

[ApiController]
[Route("api/exploration/tours")]
[Authorize]
public sealed class TourBrowsingController : ControllerBase
{
    private readonly TourBrowsingQueries _browsingQueries;

    public TourBrowsingController(TourBrowsingQueries browsingQueries)
    {
        _browsingQueries = browsingQueries;
    }

    [HttpGet("published")]
    public async Task<ActionResult<PageResult<PublishedTourDto>>> GetPublished([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return await _browsingQueries.GetPublishedAsync(page, pageSize);
    }

    [HttpGet("purchased")]
    [Authorize(Roles = "explorer")]
    public async Task<ActionResult<PageResult<TourDto>>> GetMyPurchasedTours(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return await _browsingQueries.GetMyPurchasedToursAsync(User.GetUserId(), page, pageSize);
    }

    [HttpGet("purchased/{id:guid}")]
    [Authorize(Roles = "explorer")]
    public async Task<ActionResult<PurchasedTourDetailsDto>> GetPurchasedTourDetails(Guid id)
    {
        return await _browsingQueries.GetPurchasedTourDetailsAsync(User.GetUserId(), id);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TourPreviewDto>> GetPreview(Guid id)
    {
        return await _browsingQueries.GetPreviewAsync(id, User.GetUserId());
    }
}
