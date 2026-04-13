namespace Whycespace.Shared.Contracts.Infrastructure.Health;

public record HealthCheckResult(
    string Name,
    bool IsHealthy,
    string Status,
    long ResponseTimeMs,
    string? Error = null
);
