using Shared.Domain.Exceptions;

namespace Media.Core.ImageUploading;

public sealed class ImageUploadingService
{
    private readonly IImageInspector _inspector;
    private readonly IImageStorage _storage;

    public ImageUploadingService(IImageInspector inspector, IImageStorage storage)
    {
        _inspector = inspector;
        _storage = storage;
    }

    public async Task<ImageAddressDto> UploadAsync(byte[] content)
    {
        var header = _inspector.Inspect(content)
            ?? throw new DomainException("The file is empty or is not a JPEG, PNG or WebP image.");
        var image = new ImageFile(content, header.Format, header.Width, header.Height);

        var name = await _storage.SaveAsync(image);

        return new ImageAddressDto($"/api/media/images/{name}");
    }
}
