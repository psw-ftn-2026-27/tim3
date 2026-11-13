using FluentAssertions;
using Games.Domain.SocialChallenges;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.SocialChallenges;

public class ActivePeriodTests
{
    [Fact]
    public void Active_period_cannot_start_and_end_at_the_same_time()
    {
        var creation = () => new ActivePeriod(new TimeOnly(18, 0), new TimeOnly(18, 0));

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Active_period_may_span_midnight()
    {
        var period = new ActivePeriod(new TimeOnly(22, 0), new TimeOnly(2, 0));

        period.Start.Should().Be(new TimeOnly(22, 0));
        period.End.Should().Be(new TimeOnly(2, 0));
    }
}
