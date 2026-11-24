using Games.Domain.LocationChallenges;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Games.Infrastructure.Persistence.LocationChallenges;

internal static class LocationChallengeConfiguration
{
    public static void ConfigureLocationChallenges(this ModelBuilder modelBuilder)
    {
        ConfigureLocationChallenge(modelBuilder.Entity<LocationChallenge>());
        ConfigureHiddenPoint(modelBuilder.Entity<HiddenPoint>());
    }

    private static void ConfigureLocationChallenge(EntityTypeBuilder<LocationChallenge> builder)
    {
        builder.Property(locationChallenge => locationChallenge.Name).HasMaxLength(200);

        builder.OwnsOne(locationChallenge => locationChallenge.StartLocation);

        builder.HasMany(locationChallenge => locationChallenge.HiddenPoints)
            .WithOne()
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(locationChallenge => locationChallenge.HiddenPoints).AutoInclude();
    }

    private static void ConfigureHiddenPoint(EntityTypeBuilder<HiddenPoint> builder)
    {
        builder.ToTable("HiddenPoints");
        builder.Property(hiddenPoint => hiddenPoint.Id).ValueGeneratedNever();

        builder.OwnsOne(hiddenPoint => hiddenPoint.Location);
    }
}
