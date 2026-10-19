namespace Farmora.Application.DTO.Following;

public sealed class FollowResponse
{
    public Guid CustomerId { get; init; }
    public Guid FarmerId { get; init; }
    public DateTime CreatedAtUtc { get; init; }
}

public sealed class FavoriteResponse
{
    public Guid CustomerId { get; init; }
    public Guid ProductId { get; init; }
    public DateTime CreatedAtUtc { get; init; }
}
