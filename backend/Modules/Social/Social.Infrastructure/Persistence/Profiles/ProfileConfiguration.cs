using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Social.Domain.Profiles;

namespace Social.Infrastructure.Persistence.Profiles;

internal static class ProfileConfiguration
{
    public static void ConfigureProfiles(this ModelBuilder modelBuilder)
    {
        ConfigureProfile(modelBuilder.Entity<Profile>());
        ConfigureFollowRequest(modelBuilder.Entity<FollowRequest>());
    }

    private static void ConfigureProfile(EntityTypeBuilder<Profile> builder)
    {
        builder.HasIndex(profile => profile.UserId).IsUnique();

        builder.HasMany(profile => profile.SentRequests)
            .WithOne()
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(profile => profile.SentRequests).AutoInclude();
    }

    private static void ConfigureFollowRequest(EntityTypeBuilder<FollowRequest> builder)
    {
        builder.ToTable("FollowRequests");
        builder.Property(request => request.Id).ValueGeneratedNever();
        builder.Property(request => request.Status).HasConversion<string>().HasMaxLength(20);
    }
}
