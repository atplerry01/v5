using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.CoreSystem.Reconciliation.SystemVerification;

/// <summary>
/// System truth verification authority. Manages verification sessions that compare:
/// - Event store vs projections
/// - Cross-ledger consistency
/// - SPV financial correctness
/// </summary>
public sealed class ReconciliationAggregate : AggregateRoot
{
    public VerificationScope Scope { get; private set; } = null!;
    public ReconciliationStatus Status { get; private set; } = ReconciliationStatus.Pending;
    private readonly List<ConsistencyResult> _results = [];
    public IReadOnlyList<ConsistencyResult> Results => _results.AsReadOnly();

    public static ReconciliationAggregate StartSession(Guid id, VerificationScope scope)
    {
        var agg = new ReconciliationAggregate
        {
            Id = id,
            Scope = scope,
            Status = ReconciliationStatus.InProgress
        };
        agg.RaiseDomainEvent(new VerificationSessionStartedEvent(id, scope.ScopeType, scope.TargetSystem));
        return agg;
    }

    public void RecordCheck(ConsistencyResult result)
    {
        EnsureNotTerminal(Status, s => s.IsTerminal, "RecordCheck");
        EnsureValidTransition(Status, ReconciliationStatus.InProgress, ReconciliationStatus.IsValidTransition);
        _results.Add(result);
        RaiseDomainEvent(new ConsistencyCheckRecordedEvent(Id, result.CheckName, result.IsConsistent, result.Details));
    }

    public void Complete()
    {
        EnsureNotTerminal(Status, s => s.IsTerminal, "Complete");
        var failedChecks = _results.Count(r => !r.IsConsistent);
        var allConsistent = failedChecks == 0;
        Status = ReconciliationStatus.Verified;
        RaiseDomainEvent(new VerificationSessionCompletedEvent(Id, allConsistent, _results.Count, failedChecks));
    }

    public void Fail(string reason)
    {
        EnsureNotTerminal(Status, s => s.IsTerminal, "Fail");
        Status = ReconciliationStatus.Failed;
        RaiseDomainEvent(new VerificationSessionFailedEvent(Id, reason));
    }
}
