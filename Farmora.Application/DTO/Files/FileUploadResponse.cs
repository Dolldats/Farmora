namespace Farmora.Application.DTO.Files;

public sealed class FileUploadResponse
{
    public string StorageKey { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long Size { get; init; }
}
