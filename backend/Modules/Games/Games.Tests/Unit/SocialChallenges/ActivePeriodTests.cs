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

    [Theory]
    [InlineData(24, 23)]
    [InlineData(25, 1)]
    public void Moment_in_a_period_spanning_midnight_belongs_to_the_day_the_period_started(int localDay, int localHour)
    {
        var period = new ActivePeriod(new TimeOnly(22, 0), new TimeOnly(2, 0));
        var moment = new DateTime(2026, 11, localDay, localHour, 0, 0, DateTimeKind.Local).ToUniversalTime();

        var day = period.DayOf(moment);

        day.From.Should().Be(new DateTime(2026, 11, 24, 22, 0, 0, DateTimeKind.Local).ToUniversalTime());
        day.To.Should().Be(new DateTime(2026, 11, 25, 22, 0, 0, DateTimeKind.Local).ToUniversalTime());
    }
}
