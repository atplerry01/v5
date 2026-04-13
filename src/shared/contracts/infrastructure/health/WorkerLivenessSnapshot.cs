namespace Whycespace.Shared.Contracts.Infrastructure.Health;

/// <summary>
/// phase1.5-S5.2.4 / HC-5 (WORKER-LIVENESS-01): immutable per-worker
/// liveness observation. Carries the canonical low-cardinality
/// worker name and the wall-clock timestamp of the most recent
/// successful loop iteration. <see cref="LastSuccessfulIterationAt"/>
/// is null when the worker has never reported a successful iteration
/// since process start.
/// </summary>
public sealed record WorkerLivenessSnapshot(
    string WorkerName,
    DateTimeOffset? LastSuccessfulIterationAt);
