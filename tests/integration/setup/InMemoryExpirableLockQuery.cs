using Whycespace.Shared.Contracts.Enforcement;

namespace Whycespace.Tests.Integration.Setup;

/// <summary>
/// Phase 8 B7 — in-memory <see cref="IExpirableLockQuery"/>. Shape parity
/// with <see cref="InMemoryExpirableSanctionQuery"/>; the production
/// adapter filters on <c>status='Locked'</c> at the SQL boundary, so a
/// caller simulating "lock suspended" simply removes the candidate
/// from this double until <c>Resume</c> is simulated by re-adding it.
/// </summary>
public sealed class InMemoryExpirableLockQuery : IExpirableLockQuery
{
    private readonly List<ExpirableLockCandidate> _candidates = new();

    public void Add(Guid lockId, DateTimeOffset expiresAt) =>
        _candidates.Add(new ExpirableLockCandidate(lockId, expiresAt));

    public void Remove(Guid lockId) =>
        _candidates.RemoveAll(c => c.LockId == lockId);

    public int Count => _candidates.Count;

    public Task<IReadOnlyList<ExpirableLockCandidate>> QueryExpirableAsync(
        DateTimeOffset now,
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        var expired = _candidates
            .Where(c => c.ExpiresAt <= now)
            .OrderBy(c => c.ExpiresAt)
            .Take(batchSize)
            .ToArray();
        return Task.FromResult<IReadOnlyList<ExpirableLockCandidate>>(expired);
    }
}
