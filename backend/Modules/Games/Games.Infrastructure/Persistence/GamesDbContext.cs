using Games.Application;
using Games.Domain.Quizzes;
using Games.Infrastructure.Persistence.Quizzes;
using Microsoft.EntityFrameworkCore;

namespace Games.Infrastructure.Persistence;

internal sealed class GamesDbContext : DbContext, IUnitOfWork
{
    public GamesDbContext(DbContextOptions<GamesDbContext> options) : base(options) { }

    public DbSet<Quiz> Quizzes => Set<Quiz>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("games");
        builder.ConfigureQuizzes();
    }
}
