using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.SystemReconciliation.ConsistencyCheck;

public sealed class ConsistencyCheckAggregate : AggregateRoot
{
    public ConsistencyCheckId Id { get; private set; }
    public string ScopeTarget { get; private set; } = string.Empty;
    public ConsistencyCheckStatus Status { get; private set; }
    public bool? HasDiscrepancies { get; private set; }
    public DateTimeOffset InitiatedAt { get; private set; }

    private ConsistencyCheckAggregate() { }

    public static ConsistencyCheckAggregate Initiate(
        ConsistencyCheckId id,
        string scopeTarget,
        DateTimeOffset initiatedAt)
    {
        Guard.Against(string.IsNullOrEmpty(scopeTarget), ConsistencyCheckErrors.ScopeTargetMustNotBeEmpty().Message);

        var aggregate = new ConsistencyCheckAggregate();
        aggregate.RaiseDomainEvent(new ConsistencyCheckInitiatedEvent(id, scopeTarget, initiatedAt));
        return aggregate;
    }

    public void Complete(bool hasDiscrepancies, DateTimeOffset completedAt)
    {
        Guard.Against(
            Status == ConsistencyCheckStatus.Completed || Status == ConsistencyCheckStatus.Failed,
            ConsistencyCheckErrors.CheckAlreadyTerminated().Message);

        RaiseDomainEvent(new ConsistencyCheckCompletedEvent(Id, hasDiscrepancies, completedAt));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case ConsistencyCheckInitiatedEvent e:
                Id = e.Id;
                ScopeTarget = e.ScopeTarget;
                Status = ConsistencyCheckStatus.Initiated;
                InitiatedAt = e.InitiatedAt;
                break;
            case ConsistencyCheckCompletedEvent e:
                Status = ConsistencyCheckStatus.Completed;
                HasDiscrepancies = e.HasDiscrepancies;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        Guard.Against(Id.Value is null, "ConsistencyCheck must have a non-empty Id.");
        Guard.Against(string.IsNullOrEmpty(ScopeTarget), "ConsistencyCheck must have a non-empty ScopeTarget.");
    }
}
