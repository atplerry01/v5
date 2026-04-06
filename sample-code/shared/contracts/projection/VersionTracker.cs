using System.Collections.Concurrent;

namespace Whycespace.Shared.Contracts.Projection;

/// <summary>
/// Tracks the last processed event version per aggregate for a projection.
/// Used to skip already-processed events and prevent duplicate work.
///
/// Thread-safe: uses ConcurrentDictionary for multi-consumer scenarios.
/// Deterministic: version comparison is a pure numeric check.
/// </summary>
public sealed class VersionTracker
{
    private readonly ConcurrentDictionary<string, long> _versions = new();

    /// <summary>
    /// Returns true if the event should be processed (version is newer).
    /// Returns false if the event has already been processed (skip).
    /// </summary>
    public bool ShouldProcess(string aggregateId, long eventVersion)
    {
        if (_versions.TryGetValue(aggregateId, out var lastVersion) && eventVersion <= lastVersion)
            return false;

        return true;
    }

    /// <summary>
    /// Marks the version as processed. Must be called after successful
    /// projection handling.
    /// </summary>
    public void MarkProcessed(string aggregateId, long eventVersion)
    {
        _versions.AddOrUpdate(aggregateId, eventVersion, (_, existing) => Math.Max(existing, eventVersion));
    }

    /// <summary>
    /// Resets tracking for a full projection rebuild.
    /// </summary>
    public void Reset() => _versions.Clear();

    public long? GetLastVersion(string aggregateId) =>
        _versions.TryGetValue(aggregateId, out var v) ? v : null;
}
