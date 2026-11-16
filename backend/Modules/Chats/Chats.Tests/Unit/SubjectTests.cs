using Chats.Core.Chats;
using FluentAssertions;
using Shared.Domain.Exceptions;
using Xunit;

namespace Chats.Tests.Unit;

public class SubjectTests
{
    private static readonly Guid FirstUser = Guid.Parse("00000000-0000-0000-0000-000000000002");
    private static readonly Guid SecondUser = Guid.Parse("00000000-0000-0000-0000-000000000003");

    private static Subject OpenSubject(SubjectKind kind) =>
        kind == SubjectKind.Tour ? Subject.ForTour(Guid.NewGuid()) : Subject.ForChallenge(Guid.NewGuid());

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000002", true)]
    [InlineData("00000000-0000-0000-0000-000000000003", true)]
    [InlineData("00000000-0000-0000-0000-000000000004", false)]
    public void Conversation_of_two_users_admits_only_those_two_users(string userId, bool admitted)
    {
        var subject = Subject.ForPair(FirstUser, SecondUser);

        var admits = subject.Admits(Guid.Parse(userId));

        admits.Should().Be(admitted);
    }

    [Theory]
    [InlineData(SubjectKind.Tour)]
    [InlineData(SubjectKind.Challenge)]
    public void Tour_and_challenge_chats_admit_any_user(SubjectKind kind)
    {
        var subject = OpenSubject(kind);

        var admits = subject.Admits(Guid.NewGuid());

        admits.Should().BeTrue();
    }

    [Fact]
    public void Pair_of_users_is_the_same_chat_whichever_user_wrote_first()
    {
        var startedByFirst = Subject.ForPair(FirstUser, SecondUser);

        var startedBySecond = Subject.ForPair(SecondUser, FirstUser);

        startedBySecond.Should().Be(startedByFirst);
    }

    [Fact]
    public void User_cannot_start_a_conversation_with_themselves()
    {
        var creation = () => Subject.ForPair(FirstUser, FirstUser);

        creation.Should().Throw<DomainException>();
    }
}
