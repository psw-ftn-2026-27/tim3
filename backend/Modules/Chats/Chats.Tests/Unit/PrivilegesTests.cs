using Chats.Core.Chats;
using FluentAssertions;
using Shared.Domain.Exceptions;
using Xunit;

namespace Chats.Tests.Unit;

public class PrivilegesTests
{
    private static readonly Guid Owner = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid Moderator = Guid.Parse("00000000-0000-0000-0000-000000000002");
    private static readonly Guid Participant = Guid.Parse("00000000-0000-0000-0000-000000000003");
    private static readonly Guid OtherParticipant = Guid.Parse("00000000-0000-0000-0000-000000000004");

    private static Privileges OwnerAndModerator() => new([Owner, Moderator]);

    [Fact]
    public void Only_a_privileged_user_can_moderate()
    {
        var privileges = OwnerAndModerator();

        var moderating = () => privileges.EnsureCanModerate(OtherParticipant, Participant);

        moderating.Should().Throw<DomainException>();
    }

    [Fact]
    public void Privileged_user_cannot_be_muted_and_their_message_cannot_be_deleted()
    {
        var privileges = OwnerAndModerator();

        var moderating = () => privileges.EnsureCanModerate(Owner, Moderator);

        moderating.Should().Throw<DomainException>();
    }
}
