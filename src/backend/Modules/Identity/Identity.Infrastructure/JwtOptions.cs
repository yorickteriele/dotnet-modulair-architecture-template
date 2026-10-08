namespace Identity.Infrastructure;

public sealed class JwtOptions
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "Starter";
    public string Audience { get; set; } = "Starter";
    public int LifetimeMinutes { get; set; } = 30;
}
