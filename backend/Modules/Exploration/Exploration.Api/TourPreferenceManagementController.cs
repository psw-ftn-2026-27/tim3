using Exploration.Application.TourPreferenceManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;

namespace Exploration.Api;

[ApiController]
[Route("api/exploration/tour-preferences")]
[Authorize(Roles = "explorer")]
public sealed class TourPreferenceManagementController : ControllerBase
{
    private readonly TourPreferenceManagementService _managementService;
    private readonly TourPreferenceManagementQueries _managementQueries;

    public TourPreferenceManagementController(
        TourPreferenceManagementService managementService,
        TourPreferenceManagementQueries managementQueries)
    {
        _managementService = managementService;
        _managementQueries = managementQueries;
    }

    [HttpGet("mine")]
    public async Task<ActionResult<TourPreferenceDto?>> GetMine()
    {
        return await _managementQueries.GetByExplorerAsync(User.GetUserId());
    }

    [HttpPut("mine")]
    public async Task<ActionResult> SaveMine(SaveTourPreferenceDto dto)
    {
        await _managementService.SaveAsync(User.GetUserId(), dto);
        return NoContent();
    }

    [HttpDelete("mine")]
    public async Task<ActionResult> DeleteMine()
    {
        await _managementService.DeleteAsync(User.GetUserId());
        return NoContent();
    }
}
