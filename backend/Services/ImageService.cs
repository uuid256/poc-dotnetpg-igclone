namespace InstaClone.Api.Services;

public class ImageService
{
    private static readonly HashSet<string> AllowedExtensions =
        [".jpg", ".jpeg", ".png", ".gif", ".webp"];

    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

    private readonly string _uploadPath;

    public ImageService(IConfiguration configuration)
    {
        _uploadPath = configuration["Uploads:Path"] ?? "/app/uploads";
    }

    public async Task<(string? fileName, string? error)> SaveImageAsync(IFormFile file)
    {
        if (file.Length == 0)
            return (null, "File is empty.");

        if (file.Length > MaxFileSize)
            return (null, "File exceeds 10MB limit.");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            return (null, $"File type '{extension}' is not allowed. Allowed: {string.Join(", ", AllowedExtensions)}");

        Directory.CreateDirectory(_uploadPath);

        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(_uploadPath, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return (fileName, null);
    }
}
