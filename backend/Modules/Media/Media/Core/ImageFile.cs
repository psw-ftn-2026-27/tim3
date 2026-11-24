using Shared.Domain.Exceptions;

namespace Media.Core;

public sealed record ImageFile
{
    public const int MaxContentBytes = 5 * 1024 * 1024;
    public const int MaxDimension = 4000;

    public byte[] Content { get; }
    public ImageFormat Format { get; }
    public int Width { get; }
    public int Height { get; }

    public ImageFile(byte[] content, ImageFormat format, int width, int height)
    {
        if (content.Length == 0)
        {
            throw new DomainException("The image file is empty.");
        }

        if (content.Length > MaxContentBytes)
        {
            throw new DomainException("The image file is larger than 5 MB.");
        }

        if (width > MaxDimension || height > MaxDimension)
        {
            throw new DomainException("The image is larger than 4000 x 4000 pixels.");
        }

        Content = content;
        Format = format;
        Width = width;
        Height = height;
    }
}
