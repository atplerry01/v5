using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.CoreSystem.Temporal.ScheduleReference;

public sealed class ScheduleReferenceAggregate : AggregateRoot
{
    public string ScheduleName { get; private set; } = string.Empty;
    public string CronExpression { get; private set; } = string.Empty;
    public bool IsEnabled { get; private set; }

    public static ScheduleReferenceAggregate Create(Guid id, string scheduleName, string cronExpression)
    {
        var agg = new ScheduleReferenceAggregate
        {
            Id = id,
            ScheduleName = scheduleName,
            CronExpression = cronExpression,
            IsEnabled = true
        };
        agg.RaiseDomainEvent(new ScheduleReferenceCreatedEvent(id, scheduleName, cronExpression));
        return agg;
    }

    public void Disable()
    {
        IsEnabled = false;
        RaiseDomainEvent(new ScheduleReferenceDisabledEvent(Id));
    }
}
