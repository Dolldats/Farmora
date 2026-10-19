namespace Farmora.Domain.Common;

public abstract class Entity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
}

public abstract class AuditableEntity : Entity
{
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; private set; }

    // Optimistic concurrency token (maps to xmin/rowversion in Npgsql via configuration)
    public uint RowVersion { get; private set; }

    public void MarkUpdated() => UpdatedAtUtc = DateTime.UtcNow;
}
