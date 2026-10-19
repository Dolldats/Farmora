using Farmora.Application.DTO.Common;
using Farmora.Application.DTO.Verification;

namespace Farmora.Application.Interfaces.Services;

public interface IVerificationService
{
    Task<VerificationResponse> SubmitAsync(SubmitVerificationDto dto, CancellationToken cancellationToken);
    Task<VerificationResponse?> GetMyStatusAsync(Guid farmerId, CancellationToken cancellationToken);
    Task<PagedResult<VerificationResponse>> GetQueueAsync(string? status, int page, int pageSize, CancellationToken cancellationToken);
    Task<VerificationResponse?> DecideAsync(Guid verificationId, Guid adminId, DecideVerificationDto dto, CancellationToken cancellationToken);
}
