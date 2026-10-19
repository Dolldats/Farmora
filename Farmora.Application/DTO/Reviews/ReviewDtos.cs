using Farmora.Domain.Enums;

namespace Farmora.Application.DTO.Reviews;

public sealed class CreateReviewDto
{
    public Guid OrderItemId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
}

public sealed class ModerateReviewDto
{
    public ReviewStatus Status { get; set; }
}

public sealed class ReviewResponse
{
    public Guid Id { get; init; }
    public Guid OrderItemId { get; init; }
    public Guid CustomerId { get; init; }
    public Guid ProductId { get; init; }
    public int Rating { get; init; }
    public string? Comment { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAtUtc { get; init; }
}
