using Whyce.Shared.Contracts.Infrastructure.Health;

namespace Whyce.Platform.Api.Health;

public sealed class HealthAggregator
{
    private readonly IReadOnlyList<IHealthCheck> _healthChecks;

    private static readonly HashSet<string> CriticalServices = new(StringComparer.OrdinalIgnoreCase)
    {
        "postgres",
        "kafka",
        "opa"
    };

    public HealthAggregator(IEnumerable<IHealthCheck> healthChecks)
    {
        _healthChecks = healthChecks.ToList();
    }

    public async Task<AggregatedHealthReport> CheckAllAsync()
    {
        var tasks = _healthChecks.Select(hc => hc.CheckAsync());
        var results = await Task.WhenAll(tasks);

        var status = DetermineOverallStatus(results);

        return new AggregatedHealthReport(status, results);
    }

    private static string DetermineOverallStatus(IReadOnlyList<HealthCheckResult> results)
    {
        var hasCriticalFailure = results.Any(r =>
            !r.IsHealthy && CriticalServices.Contains(r.Name));

        if (hasCriticalFailure)
            return "DOWN";

        var hasAnyFailure = results.Any(r => !r.IsHealthy);

        if (hasAnyFailure)
            return "DEGRADED";

        return "HEALTHY";
    }
}

public record AggregatedHealthReport(
    string Status,
    IReadOnlyList<HealthCheckResult> Services
);
