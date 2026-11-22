using Chats.Core.Chats;
using Microsoft.EntityFrameworkCore;

namespace Chats.Infrastructure.Persistence.Chats;

internal sealed class ChatReadRepository : IChatReadRepository
{
    private readonly ChatsDbContext _dbContext;

    public ChatReadRepository(ChatsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Guid>> GetMutedUserIdsAsync(Subject subject)
    {
        var mutedUserIds = await _dbContext.Chats
            .Where(chat => chat.Subject.Kind == subject.Kind
                && chat.Subject.FirstId == subject.FirstId
                && chat.Subject.SecondId == subject.SecondId)
            .Select(chat => EF.Property<List<Guid>>(chat, "_mutedUserIds"))
            .SingleOrDefaultAsync();

        return mutedUserIds ?? [];
    }
}
