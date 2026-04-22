using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.SystemReconciliation.ReconciliationRun;

public sealed class ReconciliationRunAggregate : AggregateRoot
{
    public ReconciliationRunId Id { get; private set; }
    public string Scope { get; private set; } = string.Empty;
    public RunStatus Status { get; private set; }
    public int ChecksProcessed { get; private set; }
    public int DiscrepanciesFound { get; private set; }

    private ReconciliationRunAggregate() { }

    public static ReconciliationRunAggregate Schedule(
        ReconciliationRunId id,
        string scope)
    {
        Guard.Against(string.IsNullOrEmpty(scope), ReconciliationRunErrors.ScopeMustNotBeEmpty().Message);

        var aggregate = new ReconciliationRunAggregate();
        aggregate.RaiseDomainEvent(new ReconciliationRunScheduledEvent(id, scope));
        return aggregate;
    }

    public void Start(DateTimeOffset startedAt)
    {
        Guard.Against(Status != RunStatus.Pending, ReconciliationRunErrors.RunNotPending().Message);

        RaiseDomainEvent(new ReconciliationRunStartedEvent(Id, startedAt));
    }

    public void Complete(int checksProcessed, int discrepanciesFound, DateTimeOffset completedAt)
    {
        Guard.Against(Status != RunStatus.Running, ReconciliationRunErrors.RunNotRunning().Message);

        RaiseDomainEvent(new ReconciliationRunCompletedEvent(Id, checksProcessed, discrepanciesFound, completedAt));
    }

    public void Abort(string reason, DateTimeOffset abortedAt)
    {
        Guard.Against(Status != RunStatus.Running, ReconciliationRunErrors.RunNotRunning().Message);
        Guard.Against(string.IsNullOrEmpty(reason), ReconciliationRunErrors.AbortReasonMustNotBeEmpty().Message);

        RaiseDomainEvent(new ReconciliationRunAbortedEvent(Id, reason, abortedAt));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case ReconciliationRunScheduledEvent e:
                Id = e.Id;
                Scope = e.Scope;
                Status = RunStatus.Pending;
                break;
            case ReconciliationRunStartedEvent:
                Status = RunStatus.Running;
                break;
            case ReconciliationRunCompletedEvent e:
                Status = RunStatus.Completed;
                ChecksProcessed = e.ChecksProcessed;
                DiscrepanciesFound = e.DiscrepanciesFound;
                break;
            case ReconciliationRunAbortedEvent:
                Status = RunStatus.Aborted;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        Guard.Against(Id.Value is null, "ReconciliationRun must have a non-empty Id.");
        Guard.Against(string.IsNullOrEmpty(Scope), "ReconciliationRun must have a non-empty Scope.");
    }
}
