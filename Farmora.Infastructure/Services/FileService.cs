using System.Text;
using Farmora.Application.DTO.Files;
using Farmora.Application.Interfaces.Services;

namespace Farmora.Infastructure.Services;

public sealed class FileService : IFileService
{
    private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        { ".jpg", ".jpeg", ".png", ".webp" };
    private static readonly HashSet<string> DocExtensions = new(StringComparer.OrdinalIgnoreCase)
        { ".jpg", ".jpeg", ".png", ".webp", ".pdf" };
    private static readonly HashSet<string> ImageContentTypes = new(StringComparer.OrdinalIgnoreCase)
        { "image/jpeg", "image/png", "image/webp" };
    private static readonly HashSet<string> DocContentTypes = new(StringComparer.OrdinalIgnoreCase)
        { "image/jpeg", "image/png", "image/webp", "application/pdf" };

    private const long MaxImageBytes = 5 * 1024 * 1024;
    private const long MaxDocBytes = 10 * 1024 * 1024;

    private readonly string _root;

    public FileService(string? uploadRoot = null)
    {
        _root = uploadRoot ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        Directory.CreateDirectory(_root);
    }

    public Task<FileUploadResponse> SaveProductImageAsync(
        Stream content, string fileName, string contentType, CancellationToken cancellationToken) =>
        SaveAsync(content, fileName, contentType, "products", ImageExtensions, ImageContentTypes, MaxImageBytes, cancellationToken);

    public Task<FileUploadResponse> SaveVerificationDocumentAsync(
        Stream content, string fileName, string contentType, CancellationToken cancellationToken) =>
        SaveAsync(content, fileName, contentType, "verifications", DocExtensions, DocContentTypes, MaxDocBytes, cancellationToken);

    private async Task<FileUploadResponse> SaveAsync(
        Stream content, string fileName, string contentType, string folder,
        HashSet<string> allowedExtensions, HashSet<string> allowedContentTypes,
        long maxBytes, CancellationToken ct)
    {
        if (content is null)
            throw new ArgumentException("Content is required.");
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name is required.");

        var ext = Path.GetExtension(fileName.Trim()).ToLowerInvariant();
        if (!allowedExtensions.Contains(ext))
            throw new InvalidOperationException($"File extension '{ext}' is not allowed.");
        var normalizedContentType = (contentType ?? string.Empty).Trim();
        if (!allowedContentTypes.Contains(normalizedContentType))
            throw new InvalidOperationException($"Content type '{contentType}' is not allowed.");

        var safeName = $"{Guid.NewGuid():N}{ext}";
        var dir = Path.Combine(_root, folder);
        Directory.CreateDirectory(dir);
        var fullPath = Path.Combine(dir, safeName);

        long size = 0;
        await using (var file = File.Create(fullPath))
        {
            var buffer = new byte[81920];
            int read;
            while ((read = await content.ReadAsync(buffer.AsMemory(0, buffer.Length), ct)) > 0)
            {
                size += read;
                if (size > maxBytes)
                {
                    file.Close();
                    File.Delete(fullPath);
                    throw new InvalidOperationException($"File exceeds the maximum size of {maxBytes} bytes.");
                }
                await file.WriteAsync(buffer.AsMemory(0, read), ct);
            }
        }

        if (size == 0)
        {
            File.Delete(fullPath);
            throw new InvalidOperationException("File is empty.");
        }

        var storageKey = "uploads/" + folder + "/" + safeName;
        return new FileUploadResponse
        {
            StorageKey = storageKey,
            Url = "/" + storageKey,
            ContentType = normalizedContentType,
            Size = size,
        };
    }
}
