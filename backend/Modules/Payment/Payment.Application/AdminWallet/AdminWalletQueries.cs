using Identity.Contracts;
using Payment.Application.Wallets;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Payment.Application.AdminWallet;

public sealed class AdminWalletQueries
{
    private readonly IWalletReadRepository _walletReadRepository;
    private readonly IIdentityApi _identityApi;

    public AdminWalletQueries(IWalletReadRepository walletReadRepository, IIdentityApi identityApi)
    {
        _walletReadRepository = walletReadRepository;
        _identityApi = identityApi;
    }

    public async Task<PageResult<UserWalletSummaryDto>> GetUsersWithWalletsAsync(string text, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var users = await _identityApi.SearchAllUsersAsync(text, page, pageSize);
        var balances = await _walletReadRepository.GetBalancesByUserIdsAsync(users.Items.Select(user => user.UserId).ToList());
        var items = users.Items.Select(user => new UserWalletSummaryDto(
            user.UserId, user.DisplayName, balances.GetValueOrDefault(user.UserId))).ToList();

        return new PageResult<UserWalletSummaryDto>(items, users.TotalCount);
    }

    public async Task<WalletDto> GetUserWalletAsync(Guid userId, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var wallet = await _walletReadRepository.GetByUserIdAsync(userId, page, pageSize);
        if (wallet is not null)
        {
            return wallet;
        }

        if (!await _identityApi.ExistsAsync(userId))
        {
            throw new NotFoundException($"Wallet for user {userId} does not exist.");
        }

        return new WalletDto(0, new PageResult<WalletTransactionDto>([], 0));
    }
}
