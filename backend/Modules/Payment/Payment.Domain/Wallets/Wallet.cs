using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Payment.Domain.Wallets;

public sealed class Wallet : AggregateRoot
{
    private readonly List<WalletTransaction> _transactions = [];

    public Guid UserId { get; private set; }
    public int Balance { get; private set; }
    public IReadOnlyCollection<WalletTransaction> Transactions => _transactions.AsReadOnly();

    private Wallet() { }

    public Wallet(Guid userId) : base(Guid.NewGuid())
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException("A wallet must have an owner.");
        }

        UserId = userId;
        Balance = 0;
    }

    public void Deposit(int amount, Guid administratorId, DateTime occurredAt)
    {
        if (amount <= 0)
        {
            throw new DomainException("The deposit amount must be positive.");
        }

        if (amount > int.MaxValue - Balance)
        {
            throw new DomainException("The deposit exceeds the maximum wallet balance.");
        }

        _transactions.Add(new WalletTransaction(amount, administratorId, occurredAt));
        Balance += amount;
    }

    public void ReverseDeposit(Guid depositId, Guid administratorId, DateTime occurredAt)
    {
        var deposit = _transactions.SingleOrDefault(transaction =>
            transaction.Id == depositId && transaction.Kind == WalletTransactionKind.Deposit)
            ?? throw new NotFoundException("The deposit does not exist in this wallet.");

        var elapsed = occurredAt - deposit.OccurredAt;
        if (elapsed < TimeSpan.Zero || elapsed > TimeSpan.FromHours(24))
        {
            throw new DomainException("The deposit can only be reversed within 24 hours.");
        }

        if (_transactions.Any(transaction => transaction.OriginalDepositId == depositId))
        {
            throw new DomainException("The deposit has already been reversed.");
        }

        if (Balance < deposit.Amount)
        {
            throw new DomainException("The wallet has insufficient EC to reverse the deposit.");
        }

        _transactions.Add(new WalletTransaction(-deposit.Amount, administratorId, occurredAt, deposit.Id));
        Balance -= deposit.Amount;
    }
}
