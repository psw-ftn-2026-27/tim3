using Microsoft.EntityFrameworkCore;
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
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("payment");
        builder.ConfigureWallets();
    }
}
