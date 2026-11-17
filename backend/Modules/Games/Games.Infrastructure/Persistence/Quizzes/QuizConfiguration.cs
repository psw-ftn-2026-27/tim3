using Games.Domain.Quizzes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Games.Infrastructure.Persistence.Quizzes;

internal static class QuizConfiguration
{
    public static void ConfigureQuizzes(this ModelBuilder modelBuilder)
    {
        ConfigureQuiz(modelBuilder.Entity<Quiz>());
        ConfigureQuestion(modelBuilder.Entity<Question>());
    }

    private static void ConfigureQuiz(EntityTypeBuilder<Quiz> builder)
    {
        builder.Property(quiz => quiz.Title).HasMaxLength(200);
        builder.Property(quiz => quiz.Status).HasConversion<string>().HasMaxLength(20);

        builder.OwnsOne(quiz => quiz.Location);

        builder.HasMany(quiz => quiz.Questions)
            .WithOne()
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(quiz => quiz.Questions).AutoInclude();
    }

    private static void ConfigureQuestion(EntityTypeBuilder<Question> builder)
    {
        builder.ToTable("Questions");
        builder.Property(question => question.Id).ValueGeneratedNever();

        builder.OwnsMany(question => question.Answers, answers =>
        {
            answers.ToJson();
        });
    }
}
