using Chats.Core.Chats;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chats.Infrastructure.Persistence.Chats;

internal static class ChatConfiguration
{
    public static void ConfigureChats(this ModelBuilder modelBuilder)
    {
        ConfigureChat(modelBuilder.Entity<Chat>());
    }

    private static void ConfigureChat(EntityTypeBuilder<Chat> builder)
    {
        builder.Property(chat => chat.Id).ValueGeneratedNever();

        builder.OwnsOne(chat => chat.Subject, subject =>
        {
            subject.Property(s => s.Kind).HasConversion<string>().HasMaxLength(20);
            // One chat per subject (P1). A tour or challenge has no second ID, so nulls must count as equal.
            subject.HasIndex(s => new { s.Kind, s.FirstId, s.SecondId }).IsUnique().AreNullsDistinct(false);
        });
    }
}
