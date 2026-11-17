using Chats.Core.Chats;
using Chats.Core.Messages;

namespace Chats.Core.ChatMessaging;

public sealed class ChatMessagingService
{
    private readonly IChatRepository _chatRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ChatMessagingService(
        IChatRepository chatRepository,
        IMessageRepository messageRepository,
        IUnitOfWork unitOfWork)
    {
        _chatRepository = chatRepository;
        _messageRepository = messageRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task SendMessageAsync(Subject subject, Guid senderId, string text)
    {
        var chat = await _chatRepository.GetBySubjectAsync(subject);
        if (chat is null)
        {
            chat = new Chat(subject);
            _chatRepository.Add(chat);
        }

        _messageRepository.Add(chat.Write(senderId, text));
        await _unitOfWork.SaveChangesAsync();
    }
}
