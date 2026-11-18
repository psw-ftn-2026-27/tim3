using Shared.Domain;

namespace Payment.Domain.Wallets;

public sealed class WalletTransaction : Entity
{
    public int Amount { get; private set; }
    public DateTime OccurredAt { get; private set; }
    public WalletTransactionKind Kind { get; private set; }
    public Guid? AdministratorId { get; private set; }
    public Guid? OriginalDepositId { get; private set; }
    public Guid? TourId { get; private set; }
    public int DiscountInEc { get; private set; }

    private WalletTransaction() { }

    internal WalletTransaction(
        WalletTransactionKind kind, int amount, Guid? administratorId, DateTime occurredAt, Guid? originalDepositId = null)
        : base(Guid.NewGuid())
    {
        Kind = kind;
        Amount = amount;
        AdministratorId = administratorId;
        OccurredAt = occurredAt;
        OriginalDepositId = originalDepositId;
    }

    internal WalletTransaction(TourCharge charge, DateTime occurredAt) : base(Guid.NewGuid())
    {
        Amount = -(charge.PriceInEc - charge.DiscountInEc);
        OccurredAt = occurredAt;
        Kind = WalletTransactionKind.Purchase;
        AdministratorId = Guid.Empty;
        TourId = charge.TourId;
        DiscountInEc = charge.DiscountInEc;
    }
}
