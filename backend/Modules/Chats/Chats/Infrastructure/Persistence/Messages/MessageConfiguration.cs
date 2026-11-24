using Chats.Core.Chats;
using Chats.Core.Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chats.Infrastructure.Persistence.Messages;

internal static class MessageConfiguration
{
    public static void ConfigureMessages(this ModelBuilder modelBuilder)
    {
        ConfigureMessage(modelBuilder.Entity<Message>());
    }

    private static void ConfigureMessage(EntityTypeBuilder<Message> builder)
    {
        builder.Property(message => message.Id).ValueGeneratedNever();
        builder.Property(message => message.Text).HasMaxLength(Message.MaxTextLength);

        builder.HasOne<Chat>().WithMany().HasForeignKey(message => message.ChatId);
    }
}
