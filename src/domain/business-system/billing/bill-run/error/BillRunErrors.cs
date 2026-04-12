namespace Whycespace.Domain.BusinessSystem.Billing.BillRun;

public static class BillRunErrors
{
    public static BillRunDomainException MissingId()
        => new("BillRunId is required and must not be empty.");

    public static BillRunDomainException ItemRequired()
        => new("BillRun must contain at least one item.");

    public static BillRunDomainException AlreadyRunning(BillRunId id)
        => new($"BillRun '{id.Value}' is already running.");

    public static BillRunDomainException AlreadyCompleted(BillRunId id)
        => new($"BillRun '{id.Value}' has already completed.");

    public static BillRunDomainException AlreadyFailed(BillRunId id)
        => new($"BillRun '{id.Value}' has already failed.");

    public static BillRunDomainException InvalidStateTransition(BillRunStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}

public sealed class BillRunDomainException : Exception
{
    public BillRunDomainException(string message) : base(message) { }
}
