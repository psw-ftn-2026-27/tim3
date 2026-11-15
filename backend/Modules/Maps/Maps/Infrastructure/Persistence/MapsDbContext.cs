using Maps.Core;
using Maps.Core.UserPositions;
using Maps.Infrastructure.Persistence.UserPositions;
using Microsoft.EntityFrameworkCore;

namespace Maps.Infrastructure.Persistence;

internal sealed class MapsDbContext : DbContext, IUnitOfWork
{
    public MapsDbContext(DbContextOptions<MapsDbContext> options) : base(options) { }

    public DbSet<UserPosition> UserPositions => Set<UserPosition>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("maps");
        builder.ConfigureUserPositions();
    }
}
