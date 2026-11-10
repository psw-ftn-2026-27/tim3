using Media.Core.ImageUploading;

namespace Media.Core;

public interface IImageStorage
{
    Task<string> SaveAsync(ImageFile image);

    Task<ImageContentDto?> FindAsync(string name);
}
