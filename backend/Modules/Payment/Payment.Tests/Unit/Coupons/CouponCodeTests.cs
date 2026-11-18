using FluentAssertions;
using Payment.Domain.Coupons;
using Shared.Domain.Exceptions;
using Xunit;

namespace Payment.Tests.Unit.Coupons;

public class CouponCodeTests
{
    [Fact]
    public void A_generated_code_has_eight_uppercase_letters_and_digits()
    {
        var code = CouponCode.Generate();

        code.Value.Should().MatchRegex("^[A-Z0-9]{8}$");
    }

    [Theory]
    [InlineData("ABCD123")]
    [InlineData("ABCD12345")]
    [InlineData("abcd1234")]
    [InlineData("ABCD-123")]
    public void A_code_of_the_wrong_length_or_with_a_forbidden_character_is_rejected(string value)
    {
        var create = () => new CouponCode(value);

        create.Should().Throw<DomainException>();
    }
}
