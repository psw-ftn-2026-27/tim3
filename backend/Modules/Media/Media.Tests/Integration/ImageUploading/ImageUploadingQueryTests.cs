using System.Net;
using FluentAssertions;
using Xunit;

namespace Media.Tests.Integration.ImageUploading;

public class ImageUploadingQueryTests : BaseIntegrationTest
{
    public ImageUploadingQueryTests(MediaApiFactory factory) : base(factory) { }

    [Theory]
    [InlineData("0123456789abcdef0123456789abcdef.png")]
    [InlineData("appsettings.json")]
    [InlineData("..%5C..%5Cappsettings.json")]
    public async Task Image_that_does_not_exist_or_has_a_name_the_server_did_not_generate_is_not_found(string name)
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync($"/api/media/images/{name}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
