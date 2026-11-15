using Microsoft.EntityFrameworkCore;
using Npgsql;
using Payment.Application;
using Payment.Domain.Wallets;
using Payment.Infrastructure.Persistence.Wallets;
using Shared.Domain.Exceptions;

namespace Payment.Infrastructure.Persistence;

internal sealed class PaymentDbContext : DbContext, IUnitOfWork
{
    private const string ConcurrentWalletChangeMessage = "The wallet changed while you were editing it. Refresh and try again.";

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
            throw new DomainException(ConcurrentWalletChangeMessage);
        }
        catch (DbUpdateException exception) when (IsDuplicateWalletCreation(exception))
        {
            // A concurrent first deposit created the recipient's wallet after our existence check.
            throw new DomainException(ConcurrentWalletChangeMessage);
        }
    }

    private static bool IsDuplicateWalletCreation(DbUpdateException exception)
    {
        return exception.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            SchemaName: "payment",
            ConstraintName: "IX_Wallets_UserId"
        };
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("payment");
        builder.ConfigureWallets();
    }
}
