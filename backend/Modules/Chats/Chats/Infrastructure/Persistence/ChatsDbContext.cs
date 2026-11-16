using Chats.Core;
using Chats.Core.Chats;
using Chats.Core.Messages;
using Chats.Infrastructure.Persistence.Chats;
using Chats.Infrastructure.Persistence.Messages;
using Microsoft.EntityFrameworkCore;

namespace Chats.Infrastructure.Persistence;

internal sealed class ChatsDbContext : DbContext, IUnitOfWork
{
    public ChatsDbContext(DbContextOptions<ChatsDbContext> options) : base(options) { }

    public DbSet<Chat> Chats => Set<Chat>();
    public DbSet<Message> Messages => Set<Message>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("chats");
        builder.ConfigureChats();
        builder.ConfigureMessages();
    }
}
