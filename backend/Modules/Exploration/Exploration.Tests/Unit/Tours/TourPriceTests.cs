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
}
