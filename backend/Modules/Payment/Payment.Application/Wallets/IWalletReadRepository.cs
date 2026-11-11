using Payment.Application.OwnWallet;

namespace Payment.Application.Wallets;

public interface IWalletReadRepository
{
    Task<WalletDto?> GetByUserIdAsync(Guid userId, int page, int pageSize);
}
