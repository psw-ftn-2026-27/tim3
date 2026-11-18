using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payment.Domain.ShoppingCarts;

namespace Payment.Infrastructure.Persistence.ShoppingCarts;

internal static class ShoppingCartConfiguration
{
    public static void ConfigureShoppingCarts(this ModelBuilder modelBuilder)
    {
        ConfigureCart(modelBuilder.Entity<ShoppingCart>());
    }

    private static void ConfigureCart(EntityTypeBuilder<ShoppingCart> builder)
    {
        builder.Property(cart => cart.Id).ValueGeneratedNever();
        builder.HasIndex(cart => cart.UserId).IsUnique();
        builder.Property<long>("_revision").HasColumnName("Revision").IsConcurrencyToken();
        builder.Ignore(cart => cart.TotalInEc);
        builder.Ignore(cart => cart.CouponIds);
        builder.PrimitiveCollection<List<Guid>>("_couponIds").HasColumnName("CouponIds").HasDefaultValueSql("'{}'::uuid[]");

        builder.OwnsMany(cart => cart.Items, items =>
        {
            items.ToTable("OrderItems");
            items.WithOwner().HasForeignKey("ShoppingCartId");
            items.HasKey("ShoppingCartId", nameof(OrderItem.TourId));
            items.Property(item => item.TourId).ValueGeneratedNever();
        });
        builder.Navigation(cart => cart.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
