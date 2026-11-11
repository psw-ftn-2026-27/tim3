using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payment.Domain.Wallets;

namespace Payment.Infrastructure.Persistence.Wallets;

internal static class WalletConfiguration
{
    public static void ConfigureWallets(this ModelBuilder modelBuilder)
    {
        ConfigureWallet(modelBuilder.Entity<Wallet>());
        ConfigureTransaction(modelBuilder.Entity<WalletTransaction>());
    }

    private static void ConfigureWallet(EntityTypeBuilder<Wallet> builder)
    {
        builder.Property(wallet => wallet.Id).ValueGeneratedNever();
        builder.HasIndex(wallet => wallet.UserId).IsUnique();
        builder.Property<uint>("Version").IsRowVersion();

        builder.HasMany(wallet => wallet.Transactions)
            .WithOne()
            .HasForeignKey("WalletId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(wallet => wallet.Transactions).UsePropertyAccessMode(PropertyAccessMode.Field);
    }

    private static void ConfigureTransaction(EntityTypeBuilder<WalletTransaction> builder)
    {
        builder.ToTable("WalletTransactions");
        builder.Property(transaction => transaction.Id).ValueGeneratedNever();
        builder.Property(transaction => transaction.Kind).HasConversion<string>().HasMaxLength(20);
    }
}
