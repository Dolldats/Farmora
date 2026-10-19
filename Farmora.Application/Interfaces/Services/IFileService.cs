using Farmora.Application.DTO.Files;

namespace Farmora.Application.Interfaces.Services;

public interface IFileService
{
    Task<FileUploadResponse> SaveProductImageAsync(
        Stream content, string fileName, string contentType, CancellationToken cancellationToken);

    Task<FileUploadResponse> SaveVerificationDocumentAsync(
        Stream content, string fileName, string contentType, CancellationToken cancellationToken);
}
