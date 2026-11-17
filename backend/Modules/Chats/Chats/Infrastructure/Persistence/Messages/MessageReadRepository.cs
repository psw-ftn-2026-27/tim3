using Chats.Core.ChatMessaging;
using Chats.Core.Chats;
using Chats.Core.Messages;
using Microsoft.EntityFrameworkCore;
using Shared.Domain;

namespace Chats.Infrastructure.Persistence.Messages;

internal sealed class MessageReadRepository : IMessageReadRepository
{
    private readonly ChatsDbContext _dbContext;

    public MessageReadRepository(ChatsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PageResult<MessageRowDto>> GetBySubjectAsync(Subject subject, int page, int pageSize)
    {
        var chatIds = _dbContext.Chats
            .Where(chat => chat.Subject.Kind == subject.Kind
                && chat.Subject.FirstId == subject.FirstId
                && chat.Subject.SecondId == subject.SecondId)
            .Select(chat => chat.Id);
        var messages = _dbContext.Messages
            .Where(message => chatIds.Contains(message.ChatId))
            .OrderByDescending(message => message.SentAt);

        var totalCount = await messages.CountAsync();
        var items = await messages
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(message => new MessageRowDto(message.Id, message.SenderId, message.Text, message.SentAt))
            .ToListAsync();

        return new PageResult<MessageRowDto>(items, totalCount);
    }
}
