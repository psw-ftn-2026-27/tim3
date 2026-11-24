using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payment.Domain.TourPurchases;

namespace Payment.Infrastructure.Persistence.TourPurchases;

internal static class TourPurchaseTokenConfiguration
{
    public static void ConfigureTourPurchaseTokens(this ModelBuilder modelBuilder)
    {
        ConfigureToken(modelBuilder.Entity<TourPurchaseToken>());
    }

    private static void ConfigureToken(EntityTypeBuilder<TourPurchaseToken> builder)
    {
        builder.Property(token => token.Id).ValueGeneratedNever();
        builder.HasIndex(token => new { token.BuyerId, token.TourId }).IsUnique();
    }
}
