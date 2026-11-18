using Exploration.Domain.Tours;
using FluentAssertions;
using Shared.Domain.Exceptions;
using Xunit;

namespace Exploration.Tests.Unit.Tours;

public class KeyPointTests
{
    [Theory]
    [InlineData("", "Opis tačke.", "/api/media/images/tacka.png", true)]
    [InlineData("Sahat kula", "   ", "/api/media/images/tacka.png", true)]
    [InlineData("Sahat kula", "Opis tačke.", "", true)]
    [InlineData("Sahat kula", "Opis tačke.", "/api/media/images/tacka.png", false)]
    public void Key_point_requires_a_name_a_description_an_image_and_a_location(string name, string description, string imageAddress, bool hasLocation)
    {
        var location = hasLocation ? new GeoLocation(45.25, 19.86) : null;

        var creation = () => new KeyPoint(name, description, imageAddress, location, "Tajna tačke.", false);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Key_point_with_an_empty_secret_has_no_secret(string secret)
    {
        var keyPoint = new KeyPoint("Sahat kula", "Opis tačke.", "/api/media/images/tacka.png", new GeoLocation(45.25, 19.86), secret, false);

        keyPoint.Secret.Should().BeNull();
    }
}
