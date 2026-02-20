using InstaClone.Api.Services;
using Microsoft.AspNetCore.Http;

namespace InstaClone.Tests;

public class ImageServiceTests
{
    private readonly ImageService _service = new();

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
