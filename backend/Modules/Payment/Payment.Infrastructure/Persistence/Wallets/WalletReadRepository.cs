using Microsoft.EntityFrameworkCore;
using Payment.Application.Wallets;
using Shared.Domain;

namespace Payment.Infrastructure.Persistence.Wallets;

internal sealed class WalletReadRepository : IWalletReadRepository
{
    private readonly PaymentDbContext _dbContext;

    public WalletReadRepository(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Dictionary<Guid, int>> GetBalancesByUserIdsAsync(List<Guid> userIds)
    {
        return _dbContext.Wallets
            .Where(wallet => userIds.Contains(wallet.UserId))
            .Select(wallet => new { wallet.UserId, wallet.Balance })
            .ToDictionaryAsync(wallet => wallet.UserId, wallet => wallet.Balance);
    }

    public Task<WalletDto?> GetByUserIdAsync(Guid userId, int page, int pageSize)
    {
        var offset = (int)Math.Min((page - 1L) * pageSize, int.MaxValue);

        return _dbContext.Wallets
            .Where(wallet => wallet.UserId == userId)
            .Select(wallet => new WalletDto(
                wallet.Balance,
                new PageResult<WalletTransactionDto>(
                    wallet.Transactions
                        .OrderByDescending(transaction => transaction.OccurredAt)
                        .ThenBy(transaction => transaction.Id)
                        .Skip(offset)
                        .Take(pageSize)
                        .Select(transaction => new WalletTransactionDto(
                            transaction.Id, transaction.Amount, transaction.OccurredAt, transaction.Kind))
                        .ToList(),
                    wallet.Transactions.Count)))
            .SingleOrDefaultAsync();
    }
}
