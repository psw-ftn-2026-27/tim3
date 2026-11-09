using Shared.Domain.Exceptions;

namespace Media.Core.ImageUploading;

public sealed class ImageUploadingQueries
{
    private readonly IImageStorage _storage;

    public ImageUploadingQueries(IImageStorage storage)
    {
        _storage = storage;
    }

    public async Task<ImageContentDto> GetAsync(string name)
    {
        return await _storage.FindAsync(name)
            ?? throw new NotFoundException($"Image {name} does not exist.");
    }
}
