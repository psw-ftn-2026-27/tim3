using Chats.Core.ChatMessaging;
using Chats.Core.Chats;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;
using Shared.Domain;

namespace Chats.Api;

// Each route names what the chat belongs to; building the Subject from the route is request binding.
[ApiController]
[Route("api/chat")]
[Authorize]
public sealed class ChatMessagingController : ControllerBase
{
    private readonly ChatMessagingService _messagingService;
    private readonly ChatMessagingQueries _messagingQueries;

    public ChatMessagingController(ChatMessagingService messagingService, ChatMessagingQueries messagingQueries)
    {
        _messagingService = messagingService;
        _messagingQueries = messagingQueries;
    }

    [HttpGet("tours/{id:guid}/messages")]
    public async Task<ActionResult<PageResult<MessageDto>>> GetTourMessages(
        Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return await _messagingQueries.GetMessagesAsync(
            new Subject(SubjectKind.Tour, id), User.GetUserId(), page, pageSize);
    }

    [HttpPost("tours/{id:guid}/messages")]
    public async Task<ActionResult> SendTourMessage(Guid id, SendMessageDto dto)
    {
        await _messagingService.SendMessageAsync(new Subject(SubjectKind.Tour, id), User.GetUserId(), dto.Text);
        return NoContent();
    }

    [HttpGet("challenges/{id:guid}/messages")]
    public async Task<ActionResult<PageResult<MessageDto>>> GetChallengeMessages(
        Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return await _messagingQueries.GetMessagesAsync(
            new Subject(SubjectKind.Challenge, id), User.GetUserId(), page, pageSize);
    }

    [HttpPost("challenges/{id:guid}/messages")]
    public async Task<ActionResult> SendChallengeMessage(Guid id, SendMessageDto dto)
    {
        await _messagingService.SendMessageAsync(new Subject(SubjectKind.Challenge, id), User.GetUserId(), dto.Text);
        return NoContent();
    }

    [HttpGet("users/{firstUserId:guid}/{secondUserId:guid}/messages")]
    public async Task<ActionResult<PageResult<MessageDto>>> GetPairMessages(
        Guid firstUserId, Guid secondUserId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return await _messagingQueries.GetMessagesAsync(
            new Subject(SubjectKind.Pair, firstUserId, secondUserId), User.GetUserId(), page, pageSize);
    }

    [HttpPost("users/{firstUserId:guid}/{secondUserId:guid}/messages")]
    public async Task<ActionResult> SendPairMessage(Guid firstUserId, Guid secondUserId, SendMessageDto dto)
    {
        await _messagingService.SendMessageAsync(
            new Subject(SubjectKind.Pair, firstUserId, secondUserId), User.GetUserId(), dto.Text);
        return NoContent();
    }
}
