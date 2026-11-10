using FluentAssertions;
using Media.Core;
using Shared.Domain.Exceptions;
using Xunit;

namespace Media.Tests.Unit;

public class ImageFileTests
{
    [Theory]
    [InlineData(0, 100, 100)]
    [InlineData(5 * 1024 * 1024 + 1, 100, 100)]
    [InlineData(1024, 4001, 100)]
    [InlineData(1024, 100, 4001)]
    public void Image_cannot_be_empty_larger_than_five_megabytes_or_larger_than_4000_pixels(
        int contentLength, int width, int height)
    {
        var content = new byte[contentLength];

        var creation = () => new ImageFile(content, ImageFormat.Png, width, height);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(1, 1, 1)]
    [InlineData(5 * 1024 * 1024, 100, 100)]
    [InlineData(1024, 4000, 4000)]
    public void Image_at_the_size_and_dimension_limits_is_accepted(int contentLength, int width, int height)
    {
        var content = new byte[contentLength];

        var creation = () => new ImageFile(content, ImageFormat.Png, width, height);

        creation.Should().NotThrow();
    }
}
