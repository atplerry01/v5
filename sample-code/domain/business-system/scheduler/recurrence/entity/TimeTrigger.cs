namespace Whycespace.Domain.BusinessSystem.Scheduler.Recurrence;

public sealed class TimeTrigger
{
    public TriggerId TriggerId { get; }
    public DateTimeOffset ScheduledTime { get; }
    public TriggerType TriggerType { get; }
    public TriggerStatus Status { get; private set; }

    public TimeTrigger(TriggerId triggerId, DateTimeOffset scheduledTime, TriggerType triggerType)
    {
        TriggerId = triggerId;
        ScheduledTime = scheduledTime;
        TriggerType = triggerType;
        Status = TriggerStatus.Armed;
    }

    public bool IsDue(DateTimeOffset now) => Status == TriggerStatus.Armed && now >= ScheduledTime;

    public void MarkFired()
    {
        Status = TriggerStatus.Fired;
    }

    public void Disarm()
    {
        Status = TriggerStatus.Disarmed;
    }
}
