namespace Whycespace.Domain.IntelligenceSystem.Observability.Health;

public sealed record HealthState(string Value)
{
    public static readonly HealthState Healthy = new("healthy");
    public static readonly HealthState Degraded = new("degraded");
    public static readonly HealthState Unhealthy = new("unhealthy");

    public bool IsOperational => this == Healthy || this == Degraded;
    public bool IsTerminal => this == Unhealthy;
}
