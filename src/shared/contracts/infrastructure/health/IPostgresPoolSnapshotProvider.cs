namespace Whycespace.Shared.Contracts.Infrastructure.Health;

/// <summary>
/// phase1.5-S5.2.4 / HC-6 (POSTGRES-POOL-HEALTH-01): in-process
/// read seam over the canonical PC-4 pool tracking.
/// Implementations MUST be non-blocking, MUST NOT open database
/// connections, and MUST NOT issue SQL probes — the snapshot is a
/// pure read of in-memory counters.
/// </summary>
public interface IPostgresPoolSnapshotProvider
{
    IReadOnlyList<PostgresPoolSnapshot> GetSnapshot();
}
