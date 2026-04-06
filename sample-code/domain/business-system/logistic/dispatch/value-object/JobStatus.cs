namespace Whycespace.Domain.BusinessSystem.Logistic.Dispatch;

public sealed record JobStatus(string Value)
{
    public static readonly JobStatus Requested = new("requested");
    public static readonly JobStatus Assigned = new("assigned");
    public static readonly JobStatus InProgress = new("in_progress");
    public static readonly JobStatus Completed = new("completed");
    public static readonly JobStatus Cancelled = new("cancelled");

    public bool IsTerminal => this == Completed || this == Cancelled;
}
