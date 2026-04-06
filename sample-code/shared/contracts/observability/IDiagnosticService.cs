namespace Whycespace.Shared.Contracts.Observability;

public interface IDiagnosticService
{
    Task<SystemDiagnosticReport> RunDiagnosticsAsync(CancellationToken cancellationToken = default);
}

public interface IHealthCheck
{
    string Name { get; }
    Task<HealthCheckResult> CheckAsync(CancellationToken cancellationToken = default);
}

public sealed record HealthCheckResult
{
    public required string Component { get; init; }
    public required HealthStatus Status { get; init; }
    public string? Message { get; init; }
}

public sealed record SystemDiagnosticReport
{
    public required DateTimeOffset Timestamp { get; init; }
    public required HealthStatus OverallStatus { get; init; }
    public required IReadOnlyList<HealthCheckResult> Results { get; init; }
}

public enum HealthStatus
{
    Healthy,
    Degraded,
    Unhealthy
}
