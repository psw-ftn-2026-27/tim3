using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;
using Shared.Domain;
using Social.Application.ClubRoster;
using Social.Application.Clubs;

namespace Social.Api;

[ApiController]
[Route("api/social/clubs")]
[Authorize(Roles = "explorer")]
public sealed class ClubRosterController : ControllerBase
{
    private readonly ClubRosterService _rosterService;
    private readonly ClubRosterQueries _rosterQueries;

    public ClubRosterController(
        ClubRosterService rosterService,
        ClubRosterQueries rosterQueries)
    {
        _rosterService = rosterService;
        _rosterQueries = rosterQueries;
    }

    [HttpGet("invitees")]
    public async Task<ActionResult<PageResult<ClubUserDto>>> SearchInvitees(
        [FromQuery] string text, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return await _rosterQueries.SearchInviteesAsync(User.GetUserId(), text, page, pageSize);
    }

    [HttpGet("invitations")]
    public async Task<ActionResult<List<ReceivedInvitationDto>>> GetMyInvitations()
    {
        return await _rosterQueries.GetMyInvitationsAsync(User.GetUserId());
    }

    [HttpGet("{id:guid}/invitations")]
    public async Task<ActionResult<List<ClubUserDto>>> GetPendingInvitations(Guid id)
    {
        return await _rosterQueries.GetPendingInvitationsAsync(User.GetUserId(), id);
    }

    [HttpPost("{id:guid}/invitations/{inviteeId:guid}")]
    public async Task<ActionResult> Invite(Guid id, Guid inviteeId)
    {
        await _rosterService.InviteAsync(User.GetUserId(), id, inviteeId);
        return NoContent();
    }

    [HttpDelete("{id:guid}/invitations/{inviteeId:guid}")]
    public async Task<ActionResult> WithdrawInvitation(Guid id, Guid inviteeId)
    {
        await _rosterService.WithdrawInvitationAsync(User.GetUserId(), id, inviteeId);
        return NoContent();
    }

    [HttpPost("{id:guid}/invitations/accept")]
    public async Task<ActionResult> AcceptInvitation(Guid id)
    {
        await _rosterService.AcceptInvitationAsync(User.GetUserId(), id);
        return NoContent();
    }

    [HttpPost("{id:guid}/invitations/decline")]
    public async Task<ActionResult> DeclineInvitation(Guid id)
    {
        await _rosterService.DeclineInvitationAsync(User.GetUserId(), id);
        return NoContent();
    }

    [HttpDelete("{id:guid}/members/{memberId:guid}")]
    public async Task<ActionResult> RemoveMember(Guid id, Guid memberId)
    {
        await _rosterService.RemoveMemberAsync(User.GetUserId(), id, memberId);
        return NoContent();
    }
}
