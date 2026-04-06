namespace Whycespace.Domain.BusinessSystem.Scheduler.Schedule;

public sealed record DeadlineStatus(string Value)
{
    public static readonly DeadlineStatus Pending = new("pending");
    public static readonly DeadlineStatus Completed = new("completed");
    public static readonly DeadlineStatus Missed = new("missed");

    public bool IsTerminal => this == Completed || this == Missed;
}
