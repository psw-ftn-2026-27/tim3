using Microsoft.EntityFrameworkCore;
using Npgsql;
using Payment.Application;
using Payment.Domain.Products;
using Payment.Domain.Wallets;
using Payment.Infrastructure.Persistence.Products;
using Payment.Infrastructure.Persistence.Wallets;
using Shared.Domain.Exceptions;

namespace Payment.Infrastructure.Persistence;

internal sealed class PaymentDbContext : DbContext, IUnitOfWork
{
    private const string ConcurrentWalletChangeMessage = "The wallet changed while you were editing it. Refresh and try again.";

    public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options) { }

    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<Product> Products => Set<Product>();

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
        catch (DbUpdateException exception) when (IsDuplicateProductCreation(exception))
        {
            var pendingChanges = ChangeTracker.Entries()
                .Where(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
                .ToList();
            if (pendingChanges.Count != 1 || pendingChanges[0].State != EntityState.Added ||
                pendingChanges[0].Entity is not Product attemptedProduct)
            {
                throw;
            }

            var savedProduct = await Products.AsNoTracking()
                .SingleAsync(product => product.TourId == attemptedProduct.TourId);
            pendingChanges[0].State = EntityState.Detached;
            savedProduct.EnsureSameOffer(attemptedProduct.Name, attemptedProduct.AuthorId, attemptedProduct.PriceInEc);
            return 0;
        }
    }

    private static bool IsDuplicateProductCreation(DbUpdateException exception)
    {
        return exception.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            SchemaName: "payment",
            ConstraintName: "IX_Products_TourId"
        };
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
        builder.ConfigureProducts();
    }
}
