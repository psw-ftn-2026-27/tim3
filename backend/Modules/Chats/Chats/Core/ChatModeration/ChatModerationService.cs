using Chats.Core.Chats;
using Chats.Core.Messages;
using Shared.Domain.Exceptions;

namespace Chats.Core.ChatModeration;

// The caller's administrator role arrives as a plain bool from the controller; who is privileged
// is decided by ChatPrivilegesLookup, never by the chat itself.
public sealed class ChatModerationService
{
    private readonly IChatRepository _chatRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly ChatPrivilegesLookup _privilegesLookup;
    private readonly IUnitOfWork _unitOfWork;

    public ChatModerationService(
        IChatRepository chatRepository,
        IMessageRepository messageRepository,
        ChatPrivilegesLookup privilegesLookup,
        IUnitOfWork unitOfWork)
    {
        _chatRepository = chatRepository;
        _messageRepository = messageRepository;
        _privilegesLookup = privilegesLookup;
        _unitOfWork = unitOfWork;
    }

    public async Task DeleteMessageAsync(Subject subject, Guid messageId, Guid moderatorId, bool moderatorIsAdministrator)
    {
        var chat = await GetChatAsync(subject);
        var message = await _messageRepository.GetByIdAsync(messageId);
        if (message is null || message.ChatId != chat.Id)
        {
            throw new NotFoundException("The message does not exist in this chat.");
        }

        var privileges = await _privilegesLookup.GetAsync(subject, moderatorId, moderatorIsAdministrator);
        privileges.EnsureCanModerate(moderatorId, message.SenderId);

        _messageRepository.Remove(message);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task MuteUserAsync(Subject subject, Guid userId, Guid moderatorId, bool moderatorIsAdministrator)
    {
        var chat = await GetChatAsync(subject);
        var privileges = await _privilegesLookup.GetAsync(subject, moderatorId, moderatorIsAdministrator);

        chat.Mute(moderatorId, userId, privileges);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UnmuteUserAsync(Subject subject, Guid userId, Guid moderatorId, bool moderatorIsAdministrator)
    {
        var chat = await GetChatAsync(subject);
        var privileges = await _privilegesLookup.GetAsync(subject, moderatorId, moderatorIsAdministrator);

        chat.Unmute(moderatorId, userId, privileges);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<Chat> GetChatAsync(Subject subject)
    {
        return await _chatRepository.GetBySubjectAsync(subject)
            ?? throw new NotFoundException("The chat does not exist.");
    }
}
