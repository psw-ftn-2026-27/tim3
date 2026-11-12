using Payment.Application.Wallets;
using Payment.Contracts;
using Payment.Domain.Wallets;

namespace Payment.Application.OwnWallet;

public sealed class OwnWalletService : IPaymentApi
{
    private readonly IWalletRepository _walletRepository;
    private readonly IUnitOfWork _unitOfWork;

    public OwnWalletService(IWalletRepository walletRepository, IUnitOfWork unitOfWork)
    {
        _walletRepository = walletRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task EnsureWalletAsync(Guid userId)
    {
        if (await _walletRepository.ExistsForUserAsync(userId))
            return;

        var wallet = new Wallet(userId);
        _walletRepository.Add(wallet);
        await _unitOfWork.SaveChangesAsync();
    }
}
