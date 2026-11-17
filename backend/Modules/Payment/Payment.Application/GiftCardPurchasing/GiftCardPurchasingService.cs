using Identity.Contracts;
using Payment.Application.GiftCards;
using Payment.Application.Wallets;
using Payment.Domain.GiftCards;
using Shared.Domain.Exceptions;

namespace Payment.Application.GiftCardPurchasing;

public sealed class GiftCardPurchasingService
{
    private readonly IWalletRepository _walletRepository;
    private readonly IGiftCardRepository _giftCardRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityApi _identityApi;

    public GiftCardPurchasingService(
        IWalletRepository walletRepository,
        IGiftCardRepository giftCardRepository,
        IUnitOfWork unitOfWork,
        IIdentityApi identityApi)
    {
        _walletRepository = walletRepository;
        _giftCardRepository = giftCardRepository;
        _unitOfWork = unitOfWork;
        _identityApi = identityApi;
    }

    public async Task BuyAsync(Guid buyerId, BuyGiftCardDto dto)
    {
        if (!await _identityApi.IsExplorerAsync(dto.RecipientId))
        {
            throw new NotFoundException($"Explorer {dto.RecipientId} does not exist.");
        }

        var wallet = await _walletRepository.GetByUserIdAsync(buyerId)
            ?? throw new NotFoundException($"Wallet for user {buyerId} does not exist.");

        var giftCard = GiftCardPurchase.Buy(wallet, dto.RecipientId, dto.Amount, dto.Message, DateTime.UtcNow);
        _giftCardRepository.Add(giftCard);
        await _unitOfWork.SaveChangesAsync();
    }
}
