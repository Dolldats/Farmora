namespace Farmora.Infastructure.Identity;

/// <summary>Startup super-admin seed. Bound from the "AdminSeed" config section.</summary>
public sealed class AdminSeedOptions
{
    public string? Email { get; set; }
    public string? Password { get; set; }
}
