using System.Text;
using FluentAssertions;
using Media.Core;
using Media.Infrastructure;
using SkiaSharp;
using Xunit;

namespace Media.Tests.Unit;

public class SkiaImageInspectorTests
{
    private static byte[] CreateImage(SKEncodedImageFormat format, int width, int height)
    {
        using var bitmap = new SKBitmap(width, height);
        bitmap.Erase(SKColors.CornflowerBlue);
        using var data = bitmap.Encode(format, 90);
        return data.ToArray();
    }

    [Fact]
    public void File_that_is_not_an_image_is_not_recognised_as_an_image_even_when_named_jpg()
    {
        var inspector = new SkiaImageInspector();
        var content = Encoding.UTF8.GetBytes("photo.jpg: this is plain text, not an image.");

        var header = inspector.Inspect(content);

        header.Should().BeNull();
    }

    [Theory]
    [InlineData(SKEncodedImageFormat.Png, ImageFormat.Png, 30, 20)]
    [InlineData(SKEncodedImageFormat.Jpeg, ImageFormat.Jpeg, 40, 10)]
    [InlineData(SKEncodedImageFormat.Webp, ImageFormat.WebP, 25, 35)]
    public void Format_and_dimensions_of_a_real_image_are_read_correctly(
        SKEncodedImageFormat encoding, ImageFormat expectedFormat, int width, int height)
    {
        var inspector = new SkiaImageInspector();
        var content = CreateImage(encoding, width, height);

        var header = inspector.Inspect(content);

        header.Should().Be(new ImageHeader(expectedFormat, width, height));
    }
}
