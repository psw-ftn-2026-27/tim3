using Identity.Contracts;
using Payment.Application.Wallets;
using Payment.Domain.Wallets;
using Shared.Domain.Exceptions;

namespace Payment.Application.AdminWallet;

public sealed class AdminWalletService
{
    private readonly IWalletRepository _walletRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityApi _identityApi;

    public AdminWalletService(IWalletRepository walletRepository, IUnitOfWork unitOfWork, IIdentityApi identityApi)
    {
        _walletRepository = walletRepository;
        _unitOfWork = unitOfWork;
        _identityApi = identityApi;
    }

    public async Task DepositECAsync(Guid userId, Guid administratorId, DepositEcDto deposit)
    {
        if (!await _identityApi.ExistsAsync(userId))
        {
            throw new NotFoundException("The recipient does not exist.");
        }

        var wallet = await _walletRepository.GetByUserIdAsync(userId);
        if (wallet is null)
        {
            wallet = new Wallet(userId);
            _walletRepository.Add(wallet);
        }

        wallet.Deposit(deposit.Amount, administratorId, DateTime.UtcNow);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ReverseDepositECAsync(Guid userId, Guid depositId, Guid administratorId)
    {
        var wallet = await _walletRepository.GetByUserIdAsync(userId)
            ?? throw new NotFoundException("The recipient's wallet does not exist.");

        wallet.ReverseDeposit(depositId, administratorId, DateTime.UtcNow);
        await _unitOfWork.SaveChangesAsync();
    }
}
