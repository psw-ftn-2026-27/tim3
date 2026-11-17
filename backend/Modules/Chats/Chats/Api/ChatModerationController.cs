using Chats.Core.ChatModeration;
using Chats.Core.Chats;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;

namespace Chats.Api;

// Building the Subject from the route and reading whether the caller is an administrator are request binding;
// who is privileged in the chat is decided in the application layer.
[ApiController]
[Route("api/chat")]
[Authorize]
public sealed class ChatModerationController : ControllerBase
{
    private readonly ChatModerationService _moderationService;

    public ChatModerationController(ChatModerationService moderationService)
    {
        _moderationService = moderationService;
    }

    [HttpDelete("tours/{id:guid}/messages/{messageId:guid}")]
    public async Task<ActionResult> DeleteTourMessage(Guid id, Guid messageId)
    {
        await _moderationService.DeleteMessageAsync(
            Subject.ForTour(id), messageId, User.GetUserId(), User.IsInRole("administrator"));
        return NoContent();
    }

    [HttpPost("tours/{id:guid}/muted-users/{userId:guid}")]
    public async Task<ActionResult> MuteUserInTourChat(Guid id, Guid userId)
    {
        await _moderationService.MuteUserAsync(
            Subject.ForTour(id), userId, User.GetUserId(), User.IsInRole("administrator"));
        return NoContent();
    }

    [HttpDelete("tours/{id:guid}/muted-users/{userId:guid}")]
    public async Task<ActionResult> UnmuteUserInTourChat(Guid id, Guid userId)
    {
        await _moderationService.UnmuteUserAsync(
            Subject.ForTour(id), userId, User.GetUserId(), User.IsInRole("administrator"));
        return NoContent();
    }

    [HttpDelete("challenges/{id:guid}/messages/{messageId:guid}")]
    public async Task<ActionResult> DeleteChallengeMessage(Guid id, Guid messageId)
    {
        await _moderationService.DeleteMessageAsync(
            Subject.ForChallenge(id), messageId, User.GetUserId(), User.IsInRole("administrator"));
        return NoContent();
    }

    [HttpPost("challenges/{id:guid}/muted-users/{userId:guid}")]
    public async Task<ActionResult> MuteUserInChallengeChat(Guid id, Guid userId)
    {
        await _moderationService.MuteUserAsync(
            Subject.ForChallenge(id), userId, User.GetUserId(), User.IsInRole("administrator"));
        return NoContent();
    }

    [HttpDelete("challenges/{id:guid}/muted-users/{userId:guid}")]
    public async Task<ActionResult> UnmuteUserInChallengeChat(Guid id, Guid userId)
    {
        await _moderationService.UnmuteUserAsync(
            Subject.ForChallenge(id), userId, User.GetUserId(), User.IsInRole("administrator"));
        return NoContent();
    }

    [HttpDelete("users/{firstUserId:guid}/{secondUserId:guid}/messages/{messageId:guid}")]
    public async Task<ActionResult> DeletePairMessage(Guid firstUserId, Guid secondUserId, Guid messageId)
    {
        await _moderationService.DeleteMessageAsync(
            Subject.ForPair(firstUserId, secondUserId), messageId, User.GetUserId(), User.IsInRole("administrator"));
        return NoContent();
    }

    [HttpPost("users/{firstUserId:guid}/{secondUserId:guid}/muted-users/{userId:guid}")]
    public async Task<ActionResult> MuteUserInPairChat(Guid firstUserId, Guid secondUserId, Guid userId)
    {
        await _moderationService.MuteUserAsync(
            Subject.ForPair(firstUserId, secondUserId), userId, User.GetUserId(), User.IsInRole("administrator"));
        return NoContent();
    }

    [HttpDelete("users/{firstUserId:guid}/{secondUserId:guid}/muted-users/{userId:guid}")]
    public async Task<ActionResult> UnmuteUserInPairChat(Guid firstUserId, Guid secondUserId, Guid userId)
    {
        await _moderationService.UnmuteUserAsync(
            Subject.ForPair(firstUserId, secondUserId), userId, User.GetUserId(), User.IsInRole("administrator"));
        return NoContent();
    }
}
