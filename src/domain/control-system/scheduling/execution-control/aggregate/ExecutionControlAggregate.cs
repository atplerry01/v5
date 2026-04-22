using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Scheduling.ExecutionControl;

public sealed class ExecutionControlAggregate : AggregateRoot
{
    public ExecutionControlId Id { get; private set; }
    public string JobInstanceId { get; private set; } = string.Empty;
    public ControlSignal Signal { get; private set; }
    public string ActorId { get; private set; } = string.Empty;
    public DateTimeOffset IssuedAt { get; private set; }
    public ControlSignalOutcome? Outcome { get; private set; }

    private ExecutionControlAggregate() { }

    public static ExecutionControlAggregate Issue(
        ExecutionControlId id,
        string jobInstanceId,
        ControlSignal signal,
        string actorId,
        DateTimeOffset issuedAt)
    {
        Guard.Against(string.IsNullOrEmpty(jobInstanceId), "ExecutionControl requires a jobInstanceId.");
        Guard.Against(string.IsNullOrEmpty(actorId), "ExecutionControl requires an actorId.");

        var aggregate = new ExecutionControlAggregate();
        aggregate.RaiseDomainEvent(new ExecutionControlSignalIssuedEvent(id, jobInstanceId, signal, actorId, issuedAt));
        return aggregate;
    }

    public void Apply(ControlSignalOutcome outcome)
    {
        Guard.Against(Outcome.HasValue, "ExecutionControl signal outcome is already recorded.");
        RaiseDomainEvent(new ExecutionControlSignalOutcomeRecordedEvent(Id, outcome));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case ExecutionControlSignalIssuedEvent e:
                Id = e.Id; JobInstanceId = e.JobInstanceId; Signal = e.Signal; ActorId = e.ActorId; IssuedAt = e.IssuedAt;
                break;
            case ExecutionControlSignalOutcomeRecordedEvent e:
                Outcome = e.Outcome;
                break;
        }
    }

    protected override void EnsureInvariants() =>
        Guard.Against(Id.Value is null, "ExecutionControl must have an Id.");
}
