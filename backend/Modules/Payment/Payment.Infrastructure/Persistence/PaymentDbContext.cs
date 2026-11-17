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
using Payment.Domain.GiftCards;
using Payment.Infrastructure.Persistence.GiftCards;
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
    public DbSet<GiftCard> GiftCards => Set<GiftCard>();

    async Task<int> IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            throw new DomainException(exception.Entries.Any(entry => entry.Entity is ShoppingCart or OrderItem)
                ? "The cart changed. Refresh and try again."
                : ConcurrentWalletChangeMessage);
        }
        catch (DbUpdateException exception) when (GetConstraintName(exception) == "IX_Products_TourId")
        {
            var product = GetPendingProduct();
            if (product is null)
            {
                throw;
            }
            return await AcceptMatchingProductAsync(product, cancellationToken);
        }
        catch (DbUpdateException exception) when (ConstraintMessage(exception) is not null)
        {
            throw new DomainException(ConstraintMessage(exception)!);
        }
    }

    private static string? GetConstraintName(DbUpdateException exception)
    {
        return exception.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            SchemaName: "payment"
        } postgres ? postgres.ConstraintName : null;
    }

    private static string? ConstraintMessage(DbUpdateException exception)
    {
        return GetConstraintName(exception) switch
        {
            "IX_Wallets_UserId" => ConcurrentWalletChangeMessage,
            "IX_ShoppingCarts_UserId" => "The cart changed. Refresh and try again.",
            "PK_OrderItems" => "The tour is already in the cart.",
            "IX_TourPurchaseTokens_BuyerId_TourId" => "You already own this tour.",
            _ => null
        };
    }

    private Product? GetPendingProduct()
    {
        var changes = ChangeTracker.Entries()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();
        return changes.Count == 1 && changes[0].State == EntityState.Added
            ? changes[0].Entity as Product
            : null;
    }

    private async Task<int> AcceptMatchingProductAsync(
        Product product, CancellationToken cancellationToken)
    {
        var savedProduct = await Products.AsNoTracking()
            .SingleAsync(saved => saved.TourId == product.TourId, cancellationToken);
        Entry(product).State = EntityState.Detached;
        savedProduct.EnsureSameOffer(product.Name, product.AuthorId, product.PriceInEc);
        return 0;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("payment");
        builder.ConfigureWallets();
        builder.ConfigureProducts();
        builder.ConfigureShoppingCarts();
        builder.ConfigureTourPurchaseTokens();
        builder.ConfigureGiftCards();
    }
}
