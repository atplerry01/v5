namespace Whycespace.Shared.Contracts.Control.Observability.SystemHealth;

public sealed record SystemHealthReadModel
{
    public Guid HealthId { get; init; }
    public string ComponentName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset LastEvaluatedAt { get; init; }
    public string? DegradationReason { get; init; }
}
