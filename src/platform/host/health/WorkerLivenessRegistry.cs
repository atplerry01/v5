using System.Collections.Concurrent;
using Whyce.Shared.Contracts.Infrastructure.Health;

namespace Whyce.Platform.Host.Health;

/// <summary>
/// phase1.5-S5.2.4 / HC-5 (WORKER-LIVENESS-01): in-process concrete
/// <see cref="IWorkerLivenessRegistry"/>. Singleton-registered.
/// Storage-only — no background thread, no timers, no pruning. The
/// registry stores one timestamp per canonical worker name; multiple
/// workers sharing a canonical name (e.g. several
/// <c>GenericKafkaProjectionConsumerWorker</c> instances all reporting
/// as "projection-consumer") cooperatively update the same slot,
/// which matches the canonical-name semantic of HC-5.
/// </summary>
public sealed class WorkerLivenessRegistry : IWorkerLivenessRegistry
{
    private readonly ConcurrentDictionary<string, DateTimeOffset> _lastSuccess =
        new(StringComparer.Ordinal);

    public void RecordSuccess(string workerName, DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workerName);
        _lastSuccess[workerName] = now;
    }

    public IReadOnlyList<WorkerLivenessSnapshot> GetSnapshots(DateTimeOffset now)
    {
        var list = new List<WorkerLivenessSnapshot>(_lastSuccess.Count);
        foreach (var kvp in _lastSuccess)
        {
            list.Add(new WorkerLivenessSnapshot(kvp.Key, kvp.Value));
        }
        return list;
    }
}
