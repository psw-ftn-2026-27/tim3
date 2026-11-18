using Exploration.Domain.Tours;
using FluentAssertions;
using Shared.Domain.Exceptions;
using Xunit;

namespace Exploration.Tests.Unit.Tours;

public class TourPriceTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Paid_tour_requires_an_amount_above_zero(int amount)
    {
        var creation = () => TourPrice.Paid(amount);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(10.5)]
    [InlineData(2147483648.0)]
    public void Paid_tour_requires_a_whole_amount_supported_by_payment(double amount)
    {
        var creation = () => TourPrice.Paid((decimal)amount);

        creation.Should().Throw<DomainException>();
    }
}
