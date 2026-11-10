using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using Media.Core.ImageUploading;
using Shared.Tests;
using SkiaSharp;
using Xunit;

namespace Media.Tests.Integration.ImageUploading;

public class ImageUploadingCommandTests : BaseIntegrationTest
{
    public ImageUploadingCommandTests(MediaApiFactory factory) : base(factory) { }

    private static byte[] CreatePng()
    {
        using var bitmap = new SKBitmap(30, 20);
        bitmap.Erase(SKColors.CornflowerBlue);
        using var data = bitmap.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    private static MultipartFormDataContent CreateForm(byte[] content, string fileName, string contentType)
    {
        var file = new ByteArrayContent(content);
        file.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        return new MultipartFormDataContent { { file, "file", fileName } };
    }

    private int StoredImageCount() =>
        Directory.Exists(Factory.StoragePath) ? Directory.GetFiles(Factory.StoragePath).Length : 0;

    [Fact]
    public async Task Signed_in_user_uploads_an_image_and_gets_an_address_from_which_it_can_be_downloaded()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var image = CreatePng();
        using var form = CreateForm(image, "vacation.png", "image/png");

        var response = await client.PostAsync("/api/media/images", form);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ImageAddressDto>(JsonOptions);
        var download = await Factory.CreateClient().GetAsync(body!.Address);
        download.StatusCode.Should().Be(HttpStatusCode.OK);
        download.Content.Headers.ContentType!.MediaType.Should().Be("image/png");
        (await download.Content.ReadAsByteArrayAsync()).Should().Equal(image);
    }

    [Fact]
    public async Task Address_of_a_stored_image_does_not_contain_the_file_name_the_user_sent()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var form = CreateForm(CreatePng(), "my-holiday-photo.png", "image/png");

        var response = await client.PostAsync("/api/media/images", form);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ImageAddressDto>(JsonOptions);
        body!.Address.Should().NotContain("my-holiday-photo");
    }

    [Fact]
    public async Task Image_larger_than_five_megabytes_is_not_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var png = CreatePng();
        var oversized = new byte[5 * 1024 * 1024 + 1];
        png.CopyTo(oversized, 0);
        using var form = CreateForm(oversized, "large.png", "image/png");
        var storedBefore = StoredImageCount();

        var response = await client.PostAsync("/api/media/images", form);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        StoredImageCount().Should().Be(storedBefore);
    }

    [Fact]
    public async Task File_that_is_not_an_image_is_not_stored_even_when_named_jpg()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var form = CreateForm(Encoding.UTF8.GetBytes("plain text, not an image"), "photo.jpg", "image/jpeg");
        var storedBefore = StoredImageCount();

        var response = await client.PostAsync("/api/media/images", form);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        StoredImageCount().Should().Be(storedBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_upload_an_image()
    {
        var client = Factory.CreateClient();
        using var form = CreateForm(CreatePng(), "vacation.png", "image/png");

        var response = await client.PostAsync("/api/media/images", form);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
