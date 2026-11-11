using Payment.Application.Wallets;
using Payment.Domain.Wallets;

namespace Payment.Application.OwnWallet;

public sealed class OwnWalletService
{
    private readonly IWalletRepository _walletRepository;
    private readonly IUnitOfWork _unitOfWork;

    public OwnWalletService(IWalletRepository walletRepository, IUnitOfWork unitOfWork)
    {
        _walletRepository = walletRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task CreateWalletAsync(Guid userId)
    {
        var wallet = new Wallet(userId);
        _walletRepository.Add(wallet);
        await _unitOfWork.SaveChangesAsync();
    }
}
