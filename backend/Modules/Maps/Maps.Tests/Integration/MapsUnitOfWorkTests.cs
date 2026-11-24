using FluentAssertions;
using Maps.Core;
using Maps.Core.UserPositions;
using Maps.Infrastructure.Persistence;
using Maps.Tests.Integration.Seeds;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Exceptions;
using Xunit;

namespace Maps.Tests.Integration;

// An approved exception to two testing rules: this test writes through a context and tests the unit
// of work on its own. Two concurrent first positions of the same user reach the unique index only
// when their requests interleave, which no single HTTP request can make happen reliably. So the test
// stores the second position directly, which is exactly what the losing request does.
public class MapsUnitOfWorkTests : BaseIntegrationTest
{
    public MapsUnitOfWorkTests(MapsApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Second_first_position_of_the_same_user_is_rejected_instead_of_failing_and_the_stored_one_stays()
    {
        var existing = UserPositionSeed.ExplorerAtFreedomSquare;
        using var context = Factory.CreateContext<MapsDbContext>();
        context.UserPositions.Add(new UserPosition(existing.UserId,
            new Coordinates(45.2517, 19.8659), new DateTime(2026, 1, 2, 12, 0, 0, DateTimeKind.Utc)));
        IUnitOfWork unitOfWork = context;

        var save = () => unitOfWork.SaveChangesAsync();

        await save.Should().ThrowAsync<DomainException>();
        using var observer = Factory.CreateContext<MapsDbContext>();
        var positions = await observer.UserPositions.Where(position => position.UserId == existing.UserId).ToListAsync();
        positions.Should().ContainSingle().Which.Id.Should().Be(existing.Id);
    }
}
