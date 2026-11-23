using Games.Application;
using Games.Domain.LocationChallenges;
using Games.Domain.QuizParticipations;
using Games.Domain.Quizzes;
using Games.Domain.SocialChallenges;
using Games.Infrastructure.Persistence.LocationChallengeParticipations;
using Games.Infrastructure.Persistence.LocationChallenges;
using Games.Infrastructure.Persistence.QuizParticipations;
using Games.Infrastructure.Persistence.Quizzes;
using Games.Infrastructure.Persistence.SocialChallenges;
using Microsoft.EntityFrameworkCore;

namespace Games.Infrastructure.Persistence;

internal sealed class GamesDbContext : DbContext, IUnitOfWork
{
    public GamesDbContext(DbContextOptions<GamesDbContext> options) : base(options) { }

    public DbSet<Quiz> Quizzes => Set<Quiz>();
    public DbSet<SocialChallenge> SocialChallenges => Set<SocialChallenge>();
    public DbSet<LocationChallenge> LocationChallenges => Set<LocationChallenge>();
    public DbSet<LocationChallengeParticipation> LocationChallengeParticipations => Set<LocationChallengeParticipation>();
    public DbSet<QuizParticipation> QuizParticipations => Set<QuizParticipation>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("games");
        builder.ConfigureQuizzes();
        builder.ConfigureSocialChallenges();
        builder.ConfigureLocationChallenges();
        builder.ConfigureLocationChallengeParticipations();
        builder.ConfigureQuizParticipations();
    }
}
