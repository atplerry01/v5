namespace Whycespace.Shared.Contracts.Infrastructure.Health;

/// <summary>
/// phase1.5-S5.2.4 / HC-5 (WORKER-LIVENESS-01): declared worker
/// liveness ceiling. A single global silence ceiling — HC-5 does not
/// introduce per-worker overrides. Read by <c>WorkersHealthCheck</c>
/// at probe time.
/// </summary>
public sealed record WorkerHealthOptions
{
    /// <summary>
    /// Maximum age, in seconds, of a worker's last successful loop
    /// iteration before <c>WorkersHealthCheck</c> declares the worker
    /// unhealthy. Must be at least 1. Default 30.
    /// </summary>
    public int MaxSilenceSeconds { get; init; } = 30;
}
