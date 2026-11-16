using Chats.Core.Messages;
using FluentAssertions;
using Shared.Domain.Exceptions;
using Xunit;

namespace Chats.Tests.Unit;

public class MessageTests
{
    [Theory]
    [InlineData(' ', 0)]
    [InlineData(' ', 3)]
    [InlineData('x', 1001)]
    public void Message_cannot_be_empty_whitespace_only_or_longer_than_a_thousand_characters(char character, int length)
    {
        var text = new string(character, length);

        var creation = () => new Message(Guid.NewGuid(), Guid.NewGuid(), text);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Message_of_exactly_a_thousand_characters_is_accepted()
    {
        var text = new string('x', 1000);

        var creation = () => new Message(Guid.NewGuid(), Guid.NewGuid(), text);

        creation.Should().NotThrow();
    }
}
