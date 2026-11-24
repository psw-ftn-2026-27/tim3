using Payment.Domain.Wallets;

namespace Payment.Domain.GiftCards;

public static class GiftCardPurchase
{
    // P5: the card is created first, so an invalid card never charges the wallet,
    // and the wallet is charged exactly the card's amount.
    public static GiftCard Buy(Wallet buyerWallet, Guid recipientId, int amount, string? message, DateTime now)
    {
        var giftCard = new GiftCard(buyerWallet.UserId, recipientId, amount, message, now);
        buyerWallet.PayForGiftCard(giftCard.Amount, now);
        return giftCard;
    }
}
