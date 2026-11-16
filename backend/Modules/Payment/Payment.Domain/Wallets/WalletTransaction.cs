using Payment.Domain.ShoppingCarts;
using Shared.Domain;

namespace Payment.Domain.Wallets;

public sealed class WalletTransaction : Entity
{
    public int Amount { get; private set; }
    public DateTime OccurredAt { get; private set; }
    public WalletTransactionKind Kind { get; private set; }
    public Guid AdministratorId { get; private set; }
    public Guid? OriginalDepositId { get; private set; }
    public Guid? TourId { get; private set; }

    private WalletTransaction() { }

    internal WalletTransaction(int amount, Guid administratorId, DateTime occurredAt, Guid? originalDepositId = null)
        : base(Guid.NewGuid())
    {
        Amount = amount;
        AdministratorId = administratorId;
        OccurredAt = occurredAt;
        Kind = originalDepositId.HasValue ? WalletTransactionKind.Reversal : WalletTransactionKind.Deposit;
        OriginalDepositId = originalDepositId;
    }

    internal WalletTransaction(OrderItem item, DateTime occurredAt) : base(Guid.NewGuid())
    {
        Amount = -item.PriceInEc;
        OccurredAt = occurredAt;
        Kind = WalletTransactionKind.Purchase;
        AdministratorId = Guid.Empty;
        TourId = item.TourId;
    }
}
