namespace Whycespace.Tests.Integration.Setup;

/// <summary>
/// Append-only ordered log of pipeline stages observed during a test execution.
/// Used by ExecutionOrderTest to assert the canonical 8-middleware + 3-fabric
/// sequence is preserved end-to-end. Thread-safe via lock; the runtime pipeline
/// is single-threaded per command, so contention is from in-memory adapters
/// running concurrent operations on different commands.
/// </summary>
public sealed class StageRecorder
{
    private readonly object _lock = new();
    private readonly List<string> _stages = new();

    public void Record(string stage)
    {
        lock (_lock) _stages.Add(stage);
    }

    public IReadOnlyList<string> Snapshot()
    {
        lock (_lock) return _stages.ToArray();
    }

    public void Clear()
    {
        lock (_lock) _stages.Clear();
    }
}
