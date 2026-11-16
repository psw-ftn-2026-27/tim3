using Shared.Domain.Exceptions;

namespace Exploration.Domain.Tours;

public sealed record TourPrice
{
    public bool IsFree { get; }
    public decimal? Amount { get; }

    private TourPrice(bool isFree, decimal? amount)
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

        return new TourPrice(false, amount);
    }
}
