using Whycespace.Shared.Contracts.Enforcement;

namespace Whycespace.Tests.Integration.Setup;

/// <summary>
/// Phase 8 B7 — in-memory <see cref="IExpirableSanctionQuery"/>. Mirrors
/// the semantics of <c>PostgresExpirableSanctionQuery</c>: returns every
/// tracked candidate whose <c>ExpiresAt &lt;= now</c>, ordered by
/// ExpiresAt ascending, bounded by <c>batchSize</c>.
///
/// <para>
/// The production query filters on <c>status='Active'</c> at the SQL
/// boundary; in this double, callers decide which candidates to register
/// — only register "expirable" ones. Removing a candidate after dispatch
/// simulates the projection updating on <c>SanctionExpiredEvent</c>.
/// </para>
/// </summary>
public sealed class InMemoryExpirableSanctionQuery : IExpirableSanctionQuery
{
    private readonly List<ExpirableSanctionCandidate> _candidates = new();

    public void Add(Guid sanctionId, DateTimeOffset expiresAt) =>
        _candidates.Add(new ExpirableSanctionCandidate(sanctionId, expiresAt));

    public void Remove(Guid sanctionId) =>
        _candidates.RemoveAll(c => c.SanctionId == sanctionId);

    public int Count => _candidates.Count;

    public Task<IReadOnlyList<ExpirableSanctionCandidate>> QueryExpirableAsync(
        DateTimeOffset now,
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        var expired = _candidates
            .Where(c => c.ExpiresAt <= now)
            .OrderBy(c => c.ExpiresAt)
            .Take(batchSize)
            .ToArray();
        return Task.FromResult<IReadOnlyList<ExpirableSanctionCandidate>>(expired);
    }
}
