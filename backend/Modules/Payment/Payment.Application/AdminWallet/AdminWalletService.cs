using Payment.Application.Wallets;
using Shared.Domain.Exceptions;

namespace Payment.Application.AdminWallet;

public sealed class AdminWalletService
{
    private readonly IWalletRepository _walletRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AdminWalletService(IWalletRepository walletRepository, IUnitOfWork unitOfWork)
    {
        _walletRepository = walletRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task DepositECAsync(Guid userId, Guid administratorId, DepositEcDto deposit)
    {
        var wallet = await _walletRepository.GetByUserIdAsync(userId)
            ?? throw new NotFoundException("The recipient's wallet does not exist.");

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
