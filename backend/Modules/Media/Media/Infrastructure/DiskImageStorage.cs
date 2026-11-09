using System.Text.RegularExpressions;
using Media.Core;
using Media.Core.ImageUploading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Media.Infrastructure;

internal sealed class DiskImageStorage : IImageStorage
{
    private static readonly Regex GeneratedName = new(@"^[0-9a-f]{32}\.(jpg|png|webp)$");

    private readonly string _folder;

    public DiskImageStorage(IConfiguration configuration, IHostEnvironment environment)
    {
        var storagePath = configuration["Media:StoragePath"];
        if (string.IsNullOrWhiteSpace(storagePath))
        {
            throw new InvalidOperationException("Configuration value Media:StoragePath is not set.");
        }

        _folder = Path.GetFullPath(Path.Combine(environment.ContentRootPath, storagePath));
    }

    public async Task<string> SaveAsync(ImageFile image)
    {
        var name = $"{Guid.NewGuid():N}.{ExtensionOf(image.Format)}";

        Directory.CreateDirectory(_folder);
        await File.WriteAllBytesAsync(Path.Combine(_folder, name), image.Content);

        return name;
    }

    public async Task<ImageContentDto?> FindAsync(string name)
    {
        if (!GeneratedName.IsMatch(name))
        {
            return null;
        }

        var path = Path.Combine(_folder, name);
        if (!File.Exists(path))
        {
            return null;
        }

        return new ImageContentDto(await File.ReadAllBytesAsync(path), ContentTypeOf(Path.GetExtension(name)));
    }

    private static string ExtensionOf(ImageFormat format) => format switch
    {
        ImageFormat.Jpeg => "jpg",
        ImageFormat.Png => "png",
        ImageFormat.WebP => "webp",
        _ => throw new ArgumentOutOfRangeException(nameof(format))
    };

    private static string ContentTypeOf(string extension) => extension switch
    {
        ".jpg" => "image/jpeg",
        ".png" => "image/png",
        ".webp" => "image/webp",
        _ => throw new ArgumentOutOfRangeException(nameof(extension))
    };
}
