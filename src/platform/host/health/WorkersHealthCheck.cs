using System.Diagnostics;
using Whycespace.Shared.Contracts.Infrastructure.Health;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Host.Health;

/// <summary>
/// phase1.5-S5.2.4 / HC-5 (WORKER-LIVENESS-01): consults
/// <see cref="IWorkerLivenessRegistry"/> at probe time and declares
/// the synthetic "workers" health check unhealthy when any required
/// worker has either never reported a successful iteration or whose
/// most recent success is older than
/// <see cref="WorkerHealthOptions.MaxSilenceSeconds"/>.
///
/// Required worker set is hardcoded for HC-5 — config-driven taxonomy
/// is reserved for a later workstream.
/// </summary>
public sealed class WorkersHealthCheck : IHealthCheck
{
    private static readonly string[] RequiredWorkers =
    {
        "outbox-sampler",
        "kafka-outbox-publisher",
        "projection-consumer",
    };

    private readonly IWorkerLivenessRegistry _registry;
    private readonly WorkerHealthOptions _options;
    private readonly IClock _clock;

    public string Name => "workers";

    public WorkersHealthCheck(
        IWorkerLivenessRegistry registry,
        WorkerHealthOptions options,
        IClock clock)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(clock);
        if (options.MaxSilenceSeconds < 1)
            throw new ArgumentOutOfRangeException(
                nameof(options), options.MaxSilenceSeconds,
                "WorkerHealthOptions.MaxSilenceSeconds must be at least 1.");

        _registry = registry;
        _options = options;
        _clock = clock;
    }

    public Task<HealthCheckResult> CheckAsync()
    {
        var sw = Stopwatch.StartNew();
        var now = _clock.UtcNow;
        var snapshots = _registry.GetSnapshots(now);
        var byName = new Dictionary<string, WorkerLivenessSnapshot>(StringComparer.Ordinal);
        foreach (var snap in snapshots) byName[snap.WorkerName] = snap;

        foreach (var worker in RequiredWorkers)
        {
            if (!byName.TryGetValue(worker, out var snap) || snap.LastSuccessfulIterationAt is null)
            {
                sw.Stop();
                return Task.FromResult(new HealthCheckResult(
                    Name, false, "DOWN", sw.ElapsedMilliseconds,
                    $"Worker '{worker}' has never reported a successful iteration."));
            }
        }

        foreach (var worker in RequiredWorkers)
        {
            var snap = byName[worker];
            var ageSeconds = (now - snap.LastSuccessfulIterationAt!.Value).TotalSeconds;
            if (ageSeconds > _options.MaxSilenceSeconds)
            {
                sw.Stop();
                return Task.FromResult(new HealthCheckResult(
                    Name, false, "DOWN", sw.ElapsedMilliseconds,
                    $"Worker '{worker}' silent for {ageSeconds:F1}s (max {_options.MaxSilenceSeconds}s)."));
            }
        }

        sw.Stop();
        return Task.FromResult(new HealthCheckResult(Name, true, "HEALTHY", sw.ElapsedMilliseconds));
    }
}
