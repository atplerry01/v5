namespace Whycespace.Shared.Contracts.Economic.Transaction.Limit;

/// <summary>
/// Phase 4 T4.1 — read-side port that maps an account id to its active
/// transaction limit. The transaction control plane (CheckLimitStep)
/// invokes this on every transaction so the workflow does not need to
/// carry LimitId in the intent. CQRS-clean: implementations consult the
/// limit projection only — never the write-side aggregate.
///
/// Returning <c>null</c> means "no active limit defined for this account",
/// which is a valid outcome (open accounts have no per-account ceiling).
/// CheckLimitStep treats null as a no-op (transaction proceeds); a found
/// limit fails the workflow on breach via the CheckLimitCommand handler
/// chain.
/// </summary>
public interface ILimitResolver
{
    Task<LimitResolution?> ResolveAsync(
        Guid accountId,
        string currency,
        CancellationToken cancellationToken = default);
}

public sealed record LimitResolution(
    Guid LimitId,
    Guid AccountId,
    string Currency,
    decimal Threshold,
    decimal CurrentUtilization,
    string Status);
