namespace Farmora.Application.DTO.User;

public sealed class UserResponse
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string? Phone { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAtUtc { get; init; }

    public UserResponse(Guid id, string email, string? phone, string status, DateTime createdAtUtc)
    {
        Id = id;
        Email = email;
        Phone = phone;
        Status = status;
        CreatedAtUtc = createdAtUtc;
    }
}
