using Payment.Application.Wallets;
using Shared.Domain.Exceptions;

namespace Payment.Application.AdminWallet;

public sealed class AdminWalletQueries
{
    private readonly IWalletReadRepository _walletReadRepository;

    public AdminWalletQueries(IWalletReadRepository walletReadRepository)
    {
        _walletReadRepository = walletReadRepository;
    }

    public async Task<WalletDto> GetUserWalletAsync(Guid userId, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        return await _walletReadRepository.GetByUserIdAsync(userId, page, pageSize)
            ?? throw new NotFoundException($"Wallet for user {userId} does not exist.");
    }
}
