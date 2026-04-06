namespace Whycespace.Domain.CoreSystem.Reconciliation.SystemVerification;

/// <summary>
/// Specification: all consistency checks in a verification session must pass.
/// Pure predicate — no side effects.
/// </summary>
public sealed class CrossLedgerConsistencySpecification
{
    public bool IsSatisfiedBy(IReadOnlyList<ConsistencyResult> results) =>
        results.Count > 0 && results.All(r => r.IsConsistent);
}
