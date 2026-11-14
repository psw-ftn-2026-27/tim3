using Maps.Core.UserPositions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maps.Infrastructure.Persistence.UserPositions;

internal static class UserPositionConfiguration
{
    public static void ConfigureUserPositions(this ModelBuilder modelBuilder)
    {
        ConfigureUserPosition(modelBuilder.Entity<UserPosition>());
    }

    private static void ConfigureUserPosition(EntityTypeBuilder<UserPosition> builder)
    {
        builder.Property(userPosition => userPosition.Id).ValueGeneratedNever();
        builder.HasIndex(userPosition => userPosition.UserId).IsUnique();

        builder.OwnsOne(userPosition => userPosition.Coordinates);
    }
}
