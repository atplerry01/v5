namespace Whycespace.Domain.BusinessSystem.Scheduler.Recurrence;

public sealed record TriggerStatus(string Value)
{
    public static readonly TriggerStatus Armed = new("armed");
    public static readonly TriggerStatus Fired = new("fired");
    public static readonly TriggerStatus Disarmed = new("disarmed");

    public bool IsTerminal => this == Fired || this == Disarmed;
}
