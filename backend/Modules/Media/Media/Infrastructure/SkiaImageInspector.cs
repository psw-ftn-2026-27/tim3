using Media.Core;
using SkiaSharp;

namespace Media.Infrastructure;

internal sealed class SkiaImageInspector : IImageInspector
{
    public ImageHeader? Inspect(byte[] content)
    {
        if (content.Length == 0)
        {
            return null;
        }

        using var data = SKData.CreateCopy(content);
        using var codec = SKCodec.Create(data);
        if (codec is null)
        {
            return null;
        }

        ImageFormat? format = codec.EncodedFormat switch
        {
            SKEncodedImageFormat.Jpeg => ImageFormat.Jpeg,
            SKEncodedImageFormat.Png => ImageFormat.Png,
            SKEncodedImageFormat.Webp => ImageFormat.WebP,
            _ => null
        };

        return format is null ? null : new ImageHeader(format.Value, codec.Info.Width, codec.Info.Height);
    }
}
