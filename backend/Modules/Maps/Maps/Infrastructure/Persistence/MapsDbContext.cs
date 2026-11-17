using Maps.Core;
using Maps.Core.UserPositions;
using Maps.Infrastructure.Persistence.UserPositions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Shared.Domain.Exceptions;

namespace Maps.Infrastructure.Persistence;

internal sealed class MapsDbContext : DbContext, IUnitOfWork
{
    private const string ConcurrentPositionChangeMessage = "Your position was set by another request at the same time. Try again.";

    public MapsDbContext(DbContextOptions<MapsDbContext> options) : base(options) { }

    public DbSet<UserPosition> UserPositions => Set<UserPosition>();

    async Task<int> IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await SaveChangesAsync();
        }
        catch (DbUpdateException exception) when (IsDuplicatePositionCreation(exception))
        {
            // A concurrent first position of the same user was stored after our existence check.
            throw new DomainException(ConcurrentPositionChangeMessage);
        }
    }

    private static bool IsDuplicatePositionCreation(DbUpdateException exception)
    {
        return exception.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            SchemaName: "maps",
            ConstraintName: "IX_UserPositions_UserId"
        };
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("maps");
        builder.ConfigureUserPositions();
    }
}
