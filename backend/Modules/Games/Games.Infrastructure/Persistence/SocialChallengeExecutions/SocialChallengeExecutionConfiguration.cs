using Games.Domain.SocialChallengeExecutions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Games.Infrastructure.Persistence.SocialChallengeExecutions;

internal static class SocialChallengeExecutionConfiguration
{
    public static void ConfigureSocialChallengeExecutions(this ModelBuilder modelBuilder)
    {
        ConfigureSocialChallengeExecution(modelBuilder.Entity<SocialChallengeExecution>());
        ConfigureSocialChallengeParticipant(modelBuilder.Entity<SocialChallengeParticipant>());
    }

    private static void ConfigureSocialChallengeExecution(EntityTypeBuilder<SocialChallengeExecution> builder)
    {
        builder.Property(execution => execution.Id).ValueGeneratedNever();
        builder.Property(execution => execution.Status).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(execution => new { execution.ChallengeId, execution.Status });

        builder.HasMany(execution => execution.Participants)
            .WithOne()
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(execution => execution.Participants).AutoInclude();
    }

    private static void ConfigureSocialChallengeParticipant(EntityTypeBuilder<SocialChallengeParticipant> builder)
    {
        builder.ToTable("SocialChallengeParticipants");
        builder.Property(participant => participant.Id).ValueGeneratedNever();
        builder.HasIndex(participant => participant.ExplorerId);
    }
}
