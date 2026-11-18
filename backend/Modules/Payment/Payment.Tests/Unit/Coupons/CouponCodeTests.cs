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

    [Fact]
    public void A_code_typed_in_lowercase_is_the_same_code_as_in_uppercase()
    {
        var code = CouponCode.FromInput("abcd1234");

        code.Should().Be(new CouponCode("ABCD1234"));
    }

    [Theory]
    [InlineData("ABCD123")]
    [InlineData("abcd-123")]
    public void Typed_text_that_cannot_be_a_code_names_no_coupon(string text)
    {
        var create = () => CouponCode.FromInput(text);

        create.Should().Throw<NotFoundException>();
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
