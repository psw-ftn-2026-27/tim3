using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payment.Domain.Coupons;

namespace Payment.Infrastructure.Persistence.Coupons;

internal static class CouponConfiguration
{
    public static void ConfigureCoupons(this ModelBuilder modelBuilder)
    {
        ConfigureCoupon(modelBuilder.Entity<Coupon>());
    }

    private static void ConfigureCoupon(EntityTypeBuilder<Coupon> builder)
    {
        builder.Property(coupon => coupon.Id).ValueGeneratedNever();
        builder.HasIndex(coupon => coupon.AuthorId);
        builder.OwnsOne(coupon => coupon.Code, code =>
        {
            code.Property(value => value.Value).HasColumnName("Code").HasMaxLength(8);
            code.HasIndex(value => value.Value).IsUnique();
        });
    }
}
