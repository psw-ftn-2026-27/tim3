using Games.Domain.LocationChallenges;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Games.Infrastructure.Persistence.LocationChallengeParticipations;

internal static class LocationChallengeParticipationConfiguration
{
    public static void ConfigureLocationChallengeParticipations(this ModelBuilder modelBuilder)
    {
        ConfigureLocationChallengeParticipation(modelBuilder.Entity<LocationChallengeParticipation>());
    }

    private static void ConfigureLocationChallengeParticipation(EntityTypeBuilder<LocationChallengeParticipation> builder)
    {
        builder.Property(participation => participation.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(participation => new { participation.ExplorerId, participation.LocationChallengeId }).IsUnique();

        builder.OwnsOne(participation => participation.SearchCircle, searchCircle =>
        {
            searchCircle.OwnsOne(circle => circle.Center);
        });

        builder.PrimitiveCollection(participation => participation.DiscoveredPointIds)
            .HasField("_discoveredPointIds");
    }
}
