using Chats.Core.Messages;

namespace Chats.Infrastructure.Persistence.Messages;

internal sealed class MessageRepository : IMessageRepository
{
    private readonly ChatsDbContext _dbContext;

    public MessageRepository(ChatsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Message?> GetByIdAsync(Guid id)
    {
        return await _dbContext.Messages.FindAsync(id);
    }

    public void Add(Message message)
    {
        _dbContext.Messages.Add(message);
    }

    public void Remove(Message message)
    {
        _dbContext.Messages.Remove(message);
    }
}
