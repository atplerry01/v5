namespace Whycespace.Domain.BusinessSystem.Integration.Job;

public static class JobErrors
{
    public static InvalidOperationException MissingId()
        => new("JobId is required and must not be empty.");

    public static InvalidOperationException MissingDescriptor()
        => new("JobDescriptor is required and must not be empty.");

    public static InvalidOperationException InvalidStateTransition(JobStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}
