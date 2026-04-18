using System.Collections.Concurrent;
using Whycespace.Shared.Contracts.Enforcement;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Host.Adapters;

/// <summary>
/// In-memory enforcement decision cache backed by <see cref="ConcurrentDictionary{TKey,TValue}"/>.
/// Entries are time-stamped at write and treated as expired after TTL.
/// Expired entries are lazily evicted on read — no background sweeper needed.
///
/// This cache closes the projection lag window for enforcement decisions.
/// It is populated by event handlers (EnforcementDetectionHandler,
/// ViolationToEscalationHandler) within milliseconds of the event, long
/// before the projection consumer materializes the read model.
///
/// Thread-safe, non-blocking, allocation-light.
/// </summary>
public sealed class InMemoryEnforcementDecisionCache : IEnforcementDecisionCache
{
    private readonly IClock _clock;
    private readonly TimeSpan _ttl;

    private readonly ConcurrentDictionary<Guid, CacheEntry<ActiveLockState>> _locks = new();
    private readonly ConcurrentDictionary<Guid, CacheEntry<ActiveViolationState>> _violations = new();
    private readonly ConcurrentDictionary<Guid, CacheEntry<ActiveRestrictionState>> _restrictions = new();

    public InMemoryEnforcementDecisionCache(IClock clock)
        : this(clock, IEnforcementDecisionCache.DefaultTtl) { }

    public InMemoryEnforcementDecisionCache(IClock clock, TimeSpan ttl)
    {
        _clock = clock;
        _ttl = ttl;
    }

    public void RecordLock(Guid subjectId, ActiveLockState state) =>
        _locks[subjectId] = new CacheEntry<ActiveLockState>(state, _clock.UtcNow);

    public void RecordViolation(Guid subjectId, ActiveViolationState state) =>
        _violations[subjectId] = new CacheEntry<ActiveViolationState>(state, _clock.UtcNow);

    public void RecordRestriction(Guid subjectId, ActiveRestrictionState state) =>
        _restrictions[subjectId] = new CacheEntry<ActiveRestrictionState>(state, _clock.UtcNow);

    public void ClearRestriction(Guid subjectId) =>
        _restrictions.TryRemove(subjectId, out _);

    public ActiveLockState? TryGetLock(Guid subjectId)
    {
        if (!_locks.TryGetValue(subjectId, out var entry))
            return null;

        if (_clock.UtcNow - entry.WrittenAt > _ttl)
        {
            _locks.TryRemove(subjectId, out _);
            return null;
        }

        return entry.Value;
    }

    public ActiveViolationState? TryGetViolation(Guid subjectId)
    {
        if (!_violations.TryGetValue(subjectId, out var entry))
            return null;

        if (_clock.UtcNow - entry.WrittenAt > _ttl)
        {
            _violations.TryRemove(subjectId, out _);
            return null;
        }

        return entry.Value;
    }

    public ActiveRestrictionState? TryGetRestriction(Guid subjectId)
    {
        if (!_restrictions.TryGetValue(subjectId, out var entry))
            return null;

        if (_clock.UtcNow - entry.WrittenAt > _ttl)
        {
            _restrictions.TryRemove(subjectId, out _);
            return null;
        }

        return entry.Value;
    }

    private readonly record struct CacheEntry<T>(T Value, DateTimeOffset WrittenAt);
}
