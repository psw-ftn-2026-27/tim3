using Chats.Core.Messages;

namespace Chats.Infrastructure.Persistence.Messages;

internal sealed class MessageRepository : IMessageRepository
{
    private readonly ChatsDbContext _dbContext;

    public MessageRepository(ChatsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Add(Message message)
    {
        _dbContext.Messages.Add(message);
    }
}
