namespace TravelPlaner.Application.Interfaces;

public interface IImageStore
{
    Task<string> SaveAsync(Stream imageStream, string fileName, CancellationToken ct = default);
    Task DeleteAsync(string filePath, CancellationToken ct = default);
    string GetPublicUrl(string filePath);
}
