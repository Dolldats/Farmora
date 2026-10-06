using Farmora.Domain.Common;
namespace Farmora.Domain.Entities;
public sealed class Role : Entity
{
    private Role() { }
    public Role(string name)
    {
        Name = name.Trim();
    }
    public string Name { get; private set; } = null!;
}
