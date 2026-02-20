using InstaClone.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace InstaClone.Tests;

public class ImageServiceTests
{
    private readonly ImageService _service;

    public ImageServiceTests()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Uploads:Path"] = Path.Combine(Path.GetTempPath(), "image-service-tests")
            })
            .Build();
        _service = new ImageService(config);
    }

    [Fact]
    public async Task SaveImageAsync_EmptyFile_ReturnsError()
    {
        var file = new FormFile(Stream.Null, 0, 0, "image", "test.jpg");

        var (fileName, error) = await _service.SaveImageAsync(file);

        Assert.Null(fileName);
        Assert.Equal("File is empty.", error);
    }

    [Fact]
    public async Task SaveImageAsync_FileTooLarge_ReturnsError()
    {
        var file = new FormFile(Stream.Null, 0, 11 * 1024 * 1024, "image", "test.jpg");

        var (fileName, error) = await _service.SaveImageAsync(file);

        Assert.Null(fileName);
        Assert.Equal("File exceeds 10MB limit.", error);
    }

    [Fact]
    public async Task SaveImageAsync_InvalidExtension_ReturnsError()
    {
        var file = new FormFile(Stream.Null, 0, 100, "image", "test.bmp");

        var (fileName, error) = await _service.SaveImageAsync(file);

        Assert.Null(fileName);
        Assert.Contains("not allowed", error);
    }
}
