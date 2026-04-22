using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Scheduling.ScheduleControl;

public sealed class ScheduleControlAggregate : AggregateRoot
{
    public ScheduleControlId Id { get; private set; }
    public string JobDefinitionId { get; private set; } = string.Empty;
    public string TriggerExpression { get; private set; } = string.Empty;
    public ScheduleStatus Status { get; private set; }

    private ScheduleControlAggregate() { }

    public static ScheduleControlAggregate Define(
        ScheduleControlId id,
        string jobDefinitionId,
        string triggerExpression)
    {
        Guard.Against(string.IsNullOrEmpty(jobDefinitionId), "ScheduleControl requires a jobDefinitionId.");
        Guard.Against(string.IsNullOrEmpty(triggerExpression), "ScheduleControl triggerExpression must not be empty.");

        var aggregate = new ScheduleControlAggregate();
        aggregate.RaiseDomainEvent(new ScheduleControlDefinedEvent(id, jobDefinitionId, triggerExpression));
        return aggregate;
    }

    public void Suspend()
    {
        Guard.Against(Status != ScheduleStatus.Active, "Only an active schedule can be suspended.");
        RaiseDomainEvent(new ScheduleControlSuspendedEvent(Id));
    }

    public void Resume()
    {
        Guard.Against(Status != ScheduleStatus.Suspended, "Only a suspended schedule can be resumed.");
        RaiseDomainEvent(new ScheduleControlResumedEvent(Id));
    }

    public void Retire()
    {
        Guard.Against(Status == ScheduleStatus.Retired, "Schedule is already retired.");
        RaiseDomainEvent(new ScheduleControlRetiredEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case ScheduleControlDefinedEvent e:
                Id = e.Id; JobDefinitionId = e.JobDefinitionId; TriggerExpression = e.TriggerExpression;
                Status = ScheduleStatus.Active;
                break;
            case ScheduleControlSuspendedEvent: Status = ScheduleStatus.Suspended; break;
            case ScheduleControlResumedEvent: Status = ScheduleStatus.Active; break;
            case ScheduleControlRetiredEvent: Status = ScheduleStatus.Retired; break;
        }
    }

    protected override void EnsureInvariants() =>
        Guard.Against(Id.Value is null, "ScheduleControl must have an Id.");
}
