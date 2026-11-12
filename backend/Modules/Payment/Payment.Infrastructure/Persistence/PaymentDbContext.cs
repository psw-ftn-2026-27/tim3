using Microsoft.EntityFrameworkCore;
using Npgsql;
using Payment.Application;
using Payment.Domain.Wallets;
using Payment.Infrastructure.Persistence.Wallets;
using Shared.Domain.Exceptions;

namespace Payment.Infrastructure.Persistence;

internal sealed class PaymentDbContext : DbContext, IUnitOfWork
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options) { }

    public DbSet<Wallet> Wallets => Set<Wallet>();

    async Task<int> IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new DomainException("The wallet changed while you were editing it. Refresh and try again.");
        }
        catch (DbUpdateException exception) when (IsDuplicateWalletCreation(exception))
        {
            // Another request saved the wallet after our existence check.
            exception.Entries[0].State = EntityState.Detached;
            return 0;
        }
    }

    private bool IsDuplicateWalletCreation(DbUpdateException exception)
    {
        return exception.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation,
                SchemaName: "payment",
                ConstraintName: "IX_Wallets_UserId"
            }
            && exception.Entries.Count == 1
            && exception.Entries[0].Entity is Wallet
            && exception.Entries[0].State == EntityState.Added
            && ChangeTracker.Entries().Count(entry =>
                entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted) == 1;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("payment");
        builder.ConfigureWallets();
    }
}
