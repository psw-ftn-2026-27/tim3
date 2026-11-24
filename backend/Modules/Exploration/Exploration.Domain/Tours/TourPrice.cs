using Shared.Domain.Exceptions;

namespace Exploration.Domain.Tours;

public sealed record TourPrice
{
    public bool IsFree { get; }
    public int? Amount { get; }

    private TourPrice(bool isFree, int? amount)
    {
        IsFree = isFree;
        Amount = amount;
    }

    public static TourPrice Free()
    {
        return new TourPrice(true, null);
    }

    public static TourPrice Paid(decimal amount)
    {
        if (amount <= 0)
        {
            throw new DomainException("A paid tour requires an amount above zero.");
        }

        if (amount != decimal.Truncate(amount) || amount > int.MaxValue)
        {
            throw new DomainException($"The tour price must be a whole amount in EC no greater than {int.MaxValue}.");
        }

        return new TourPrice(false, (int)amount);
    }
}
