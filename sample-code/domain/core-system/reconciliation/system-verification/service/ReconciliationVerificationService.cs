namespace Whycespace.Domain.CoreSystem.Reconciliation.SystemVerification;

/// <summary>
/// Domain service for reconciliation verification evaluation.
/// Stateless — all data passed as parameters.
/// </summary>
public sealed class ReconciliationVerificationService
{
    public bool IsSessionHealthy(ReconciliationAggregate session) =>
        session.Status == ReconciliationStatus.InProgress
        || (session.Status == ReconciliationStatus.Verified && session.Results.All(r => r.IsConsistent));

    public bool HasFailures(ReconciliationAggregate session) =>
        session.Results.Any(r => !r.IsConsistent);

    public int FailureCount(ReconciliationAggregate session) =>
        session.Results.Count(r => !r.IsConsistent);
}
