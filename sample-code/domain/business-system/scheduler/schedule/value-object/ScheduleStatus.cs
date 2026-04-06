namespace Whycespace.Domain.BusinessSystem.Scheduler.Schedule;

public sealed record ScheduleStatus(string Value)
{
    public static readonly ScheduleStatus Active = new("active");
    public static readonly ScheduleStatus Paused = new("paused");
    public static readonly ScheduleStatus Expired = new("expired");
    public static readonly ScheduleStatus Cancelled = new("cancelled");

    public bool IsTerminal => this == Expired || this == Cancelled;
}
