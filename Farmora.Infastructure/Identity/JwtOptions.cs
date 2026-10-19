namespace Farmora.Infastructure.Identity;

public sealed class JwtOptions
{
    public string Issuer { get; set; } = "Farmora";
    public string Audience { get; set; } = "Farmora";
    public string Secret { get; set; } = string.Empty;
    public int AccessMinutes { get; set; } = 15;
    public int RefreshDays { get; set; } = 7;
}
