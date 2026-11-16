using Chats.Core.Chats;
using Microsoft.EntityFrameworkCore;

namespace Chats.Infrastructure.Persistence.Chats;

internal sealed class ChatRepository : IChatRepository
{
    private readonly ChatsDbContext _dbContext;

    public ChatRepository(ChatsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Chat?> GetBySubjectAsync(Subject subject)
    {
        return _dbContext.Chats.SingleOrDefaultAsync(chat =>
            chat.Subject.Kind == subject.Kind
            && chat.Subject.FirstId == subject.FirstId
            && chat.Subject.SecondId == subject.SecondId);
    }

    public void Add(Chat chat)
    {
        _dbContext.Chats.Add(chat);
    }
}
