using Media.Core;
using Media.Core.ImageUploading;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Media.Api;

[ApiController]
[Route("api/media/images")]
public sealed class ImageUploadingController : ControllerBase
{
    private const long MaxRequestBytes = ImageFile.MaxContentBytes + 64 * 1024;

    private readonly ImageUploadingService _uploadingService;
    private readonly ImageUploadingQueries _uploadingQueries;

    public ImageUploadingController(
        ImageUploadingService uploadingService,
        ImageUploadingQueries uploadingQueries)
    {
        _uploadingService = uploadingService;
        _uploadingQueries = uploadingQueries;
    }

    [HttpPost]
    [Authorize]
    [RequestSizeLimit(MaxRequestBytes)]
    public async Task<ActionResult<ImageAddressDto>> Upload(IFormFile? file)
    {
        return await _uploadingService.UploadAsync(await ReadContentAsync(file));
    }

    [HttpGet("{name}")]
    [AllowAnonymous]
    public async Task<ActionResult> Get(string name)
    {
        var image = await _uploadingQueries.GetAsync(name);
        return File(image.Content, image.ContentType);
    }

    private static async Task<byte[]> ReadContentAsync(IFormFile? file)
    {
        if (file is null)
        {
            return [];
        }

        using var content = new MemoryStream();
        await file.CopyToAsync(content);
        return content.ToArray();
    }
}
