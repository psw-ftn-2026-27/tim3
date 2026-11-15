using Exploration.Domain.Tours;
using FluentAssertions;
using Shared.Domain.Exceptions;
using Xunit;

namespace Exploration.Tests.Unit.Tours;

public class GroupSizeTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Recommended_group_size_starts_from_at_least_one_person(int from)
    {
        var creation = () => new GroupSize(from, 6);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Recommended_group_size_cannot_end_below_its_start()
    {
        var creation = () => new GroupSize(6, 5);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Recommended_group_size_may_state_an_exact_number_of_people()
    {
        var groupSize = new GroupSize(4, 4);

        groupSize.From.Should().Be(4);
        groupSize.To.Should().Be(4);
    }
}
