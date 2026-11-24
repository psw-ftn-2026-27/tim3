namespace Payment.Application.Wallets;

public interface IWalletReadRepository
{
    Task<Dictionary<Guid, int>> GetBalancesByUserIdsAsync(List<Guid> userIds);
    Task<WalletDto?> GetByUserIdAsync(Guid userId, int page, int pageSize);
}
