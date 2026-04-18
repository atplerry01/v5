using System.Collections.Concurrent;

namespace Whycespace.Tests.Integration.EconomicSystem.Certification.Shared;

/// <summary>
/// In-memory dead-letter queue for certification tests. Captures the
/// logical identity of any dispatch that exhausts its in-test retry
/// budget (command + aggregate id + command id + reason) so a later
/// replay stage can rebuild a fresh <c>CommandContext</c> — write-once
/// fields on the original context prevent reusing it verbatim — and
/// re-dispatch against a recovered runtime.
///
/// Capture is append-only and preserves submission order. Replay is
/// idempotent by construction because the idempotency middleware keys
/// on (command type, CommandId); re-dispatching the same logical
/// command twice is rejected by the middleware on the second attempt.
/// </summary>
public sealed class DeadLetterQueue
{
    private readonly ConcurrentQueue<DlqEntry> _entries = new();

    public int Count => _entries.Count;

    public void Capture(object command, Guid aggregateId, string reason)
    {
        _entries.Enqueue(new DlqEntry(command, aggregateId, reason));
    }

    public IReadOnlyList<DlqEntry> Snapshot() => _entries.ToArray();

    public IReadOnlyList<DlqEntry> Drain()
    {
        var drained = new List<DlqEntry>();
        while (_entries.TryDequeue(out var entry))
            drained.Add(entry);
        return drained;
    }
}

public sealed record DlqEntry(object Command, Guid AggregateId, string Reason);
