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
}
