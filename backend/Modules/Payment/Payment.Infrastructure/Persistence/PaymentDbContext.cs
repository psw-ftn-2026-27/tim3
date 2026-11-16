using Microsoft.EntityFrameworkCore;
using Npgsql;
using Payment.Application;
using Payment.Domain.Products;
using Payment.Domain.ShoppingCarts;
using Payment.Domain.TourPurchases;
using Payment.Domain.Wallets;
using Payment.Infrastructure.Persistence.Products;
using Payment.Infrastructure.Persistence.ShoppingCarts;
using Payment.Infrastructure.Persistence.TourPurchases;
using Payment.Infrastructure.Persistence.Wallets;
using Shared.Domain.Exceptions;

namespace Payment.Infrastructure.Persistence;

internal sealed class PaymentDbContext : DbContext, IUnitOfWork
{
    private const string ConcurrentWalletChangeMessage = "The wallet changed while you were editing it. Refresh and try again.";

    public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options) { }

    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ShoppingCart> ShoppingCarts => Set<ShoppingCart>();
    public DbSet<TourPurchaseToken> TourPurchaseTokens => Set<TourPurchaseToken>();

    async Task<int> IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException exception) when (exception.Entries.Any(entry => entry.Entity is ShoppingCart or OrderItem))
        {
            throw new DomainException("Korpa je izmenjena. Osveži je i pokušaj ponovo.");
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
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            SchemaName: "payment",
            ConstraintName: "IX_ShoppingCarts_UserId"
        })
        {
            throw new DomainException("Korpa je izmenjena. Osveži je i pokušaj ponovo.");
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            SchemaName: "payment",
            ConstraintName: "PK_OrderItems"
        })
        {
            throw new DomainException("Tura je već u korpi.");
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            SchemaName: "payment",
            ConstraintName: "IX_TourPurchaseTokens_BuyerId_TourId"
        })
        {
            throw new DomainException("Već poseduješ ovu turu.");
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
        builder.ConfigureShoppingCarts();
        builder.ConfigureTourPurchaseTokens();
    }
}
