using Games.Domain.QuizParticipations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Games.Infrastructure.Persistence.QuizParticipations;

internal static class QuizParticipationConfiguration
{
    public static void ConfigureQuizParticipations(this ModelBuilder modelBuilder)
    {
        ConfigureQuizParticipation(modelBuilder.Entity<QuizParticipation>());
    }

    private static void ConfigureQuizParticipation(EntityTypeBuilder<QuizParticipation> builder)
    {
        builder.Property(participation => participation.Id).ValueGeneratedNever();
        builder.HasIndex(participation => new { participation.QuizId, participation.ExplorerId }).IsUnique();

        builder.Ignore(participation => participation.PassQuestionIds);
        builder.PrimitiveCollection<List<Guid>>("_passQuestionIds")
            .HasColumnName("PassQuestionIds")
            .HasDefaultValueSql("'{}'::uuid[]");

        builder.OwnsMany(participation => participation.Answers, answers =>
        {
            answers.ToJson();
        });
    }
}
