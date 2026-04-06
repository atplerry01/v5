using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.BusinessSystem.Scheduler.Schedule;

public sealed class ScheduleAggregate : AggregateRoot
{
    public RecurrenceRule RecurrenceRule { get; private set; } = default!;
    public DateTimeOffset StartTime { get; private set; }
    public DateTimeOffset? EndTime { get; private set; }
    public ScheduleStatus Status { get; private set; } = ScheduleStatus.Active;
    public DateTimeOffset? LastTriggeredAt { get; private set; }

    public static ScheduleAggregate Create(
        Guid scheduleId,
        string cronExpression,
        string timeZone,
        DateTimeOffset startTime,
        DateTimeOffset? endTime)
    {
        if (endTime.HasValue && endTime.Value <= startTime)
            throw new DomainException(ScheduleErrors.InvalidTimeRange, "End time must be after start time.");

        if (string.IsNullOrWhiteSpace(cronExpression) && !endTime.HasValue)
            throw new DomainException(ScheduleErrors.InvalidRecurrenceRule, "Non-recurring schedules must have an end time.");

        var schedule = new ScheduleAggregate();
        schedule.Apply(new ScheduleCreatedEvent(scheduleId, cronExpression, timeZone, startTime, endTime));
        return schedule;
    }

    public void RecordTrigger(DateTimeOffset triggeredAt)
    {
        if (Status.IsTerminal)
            throw new DomainException(ScheduleErrors.InvalidTransition, $"Cannot trigger schedule in '{Status.Value}' status.");

        Apply(new ScheduleTriggeredEvent(Id, triggeredAt));
    }

    public void Cancel()
    {
        if (Status == ScheduleStatus.Cancelled)
            throw new DomainException(ScheduleErrors.AlreadyCancelled, "Schedule is already cancelled.");

        if (Status.IsTerminal)
            throw new DomainException(ScheduleErrors.InvalidTransition, $"Cannot cancel schedule in '{Status.Value}' status.");

        Apply(new ScheduleCancelledEvent(Id));
    }

    private void Apply(ScheduleCreatedEvent e)
    {
        Id = e.ScheduleId;
        RecurrenceRule = new RecurrenceRule(e.CronExpression, e.TimeZone);
        StartTime = e.StartTime;
        EndTime = e.EndTime;
        Status = ScheduleStatus.Active;
        RaiseDomainEvent(e);
    }

    private void Apply(ScheduleTriggeredEvent e)
    {
        LastTriggeredAt = e.TriggeredAt;
        RaiseDomainEvent(e);
    }

    private void Apply(ScheduleCancelledEvent e)
    {
        Status = ScheduleStatus.Cancelled;
        RaiseDomainEvent(e);
    }
}
