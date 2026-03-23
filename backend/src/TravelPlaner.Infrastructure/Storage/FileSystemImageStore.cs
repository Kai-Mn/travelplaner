using Microsoft.Extensions.Options;
using TravelPlaner.Application.Interfaces;

namespace TravelPlaner.Infrastructure.Storage;

public class ImageStoreOptions
{
    public const string Section = "ImageStore";
    public string BasePath { get; set; } = "/data/images";
    public string BaseUrl { get; set; } = "/images";
}

public class FileSystemImageStore(IOptions<ImageStoreOptions> options) : IImageStore
{
    private readonly ImageStoreOptions _opts = options.Value;

    public async Task<string> SaveAsync(Stream imageStream, string fileName, CancellationToken ct)
    {
        Directory.CreateDirectory(_opts.BasePath);

        var ext = Path.GetExtension(fileName);
        var storedName = $"{Guid.NewGuid()}{ext}";
        var fullPath = Path.Combine(_opts.BasePath, storedName);

        await using var fs = File.Create(fullPath);
        await imageStream.CopyToAsync(fs, ct);

        return storedName;
    }

    public Task DeleteAsync(string filePath, CancellationToken ct)
    {
        var fullPath = Path.Combine(_opts.BasePath, filePath);
        if (File.Exists(fullPath))
            File.Delete(fullPath);
        return Task.CompletedTask;
    }

    public string GetPublicUrl(string filePath)
        => $"{_opts.BaseUrl}/{filePath}";
}
