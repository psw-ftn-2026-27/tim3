using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Payment.Domain.GiftCards;

public sealed class GiftCard : AggregateRoot
{
    private const int MinAmount = 10;
    private const int MaxAmount = 1000;
    private const int MaxMessageLength = 200;

    public Guid BuyerId { get; private set; }
    public Guid RecipientId { get; private set; }
    public int Amount { get; private set; }
    public string? Message { get; private set; }
    public DateTime PurchasedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RedeemedAt { get; private set; }

    private GiftCard() { }

    public GiftCard(Guid buyerId, Guid recipientId, int amount, string? message, DateTime purchasedAt)
        : base(Guid.NewGuid())
    {
        if (buyerId == Guid.Empty || recipientId == Guid.Empty)
        {
            throw new DomainException("A gift card must have a buyer and a recipient.");
        }

        if (recipientId == buyerId)
        {
            throw new DomainException("A gift card cannot be bought for yourself.");
        }

        if (amount < MinAmount || amount > MaxAmount)
        {
            throw new DomainException($"The gift card amount must be between {MinAmount} and {MaxAmount} EC.");
        }

        if (message is not null && message.Length > MaxMessageLength)
        {
            throw new DomainException($"The gift card message can have at most {MaxMessageLength} characters.");
        }

        BuyerId = buyerId;
        RecipientId = recipientId;
        Amount = amount;
        Message = message;
        PurchasedAt = purchasedAt;
        ExpiresAt = purchasedAt.AddYears(3);
    }

    // P1: only the recipient can use the card, and only while it is valid.
    public bool CanBeUsedBy(Guid buyerId, DateTime moment)
    {
        return buyerId == RecipientId && StatusAt(moment) == GiftCardStatus.Valid;
    }

    // P5: the card is spent once, in full.
    public void Redeem(Guid buyerId, DateTime moment)
    {
        if (!CanBeUsedBy(buyerId, moment))
        {
            throw new DomainException("The gift card cannot be used.");
        }

        RedeemedAt = moment;
    }

    // A redeemed card stays redeemed after it would have expired.
    public GiftCardStatus StatusAt(DateTime moment)
    {
        if (RedeemedAt is not null)
        {
            return GiftCardStatus.Redeemed;
        }

        return moment < ExpiresAt ? GiftCardStatus.Valid : GiftCardStatus.Expired;
    }
}
