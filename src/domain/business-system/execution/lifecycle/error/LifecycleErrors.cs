namespace Whycespace.Domain.BusinessSystem.Execution.Lifecycle;

public static class LifecycleErrors
{
    public static LifecycleDomainException MissingId()
        => new("LifecycleId is required and must not be empty.");

    public static LifecycleDomainException MissingSubjectId()
        => new("LifecycleSubjectId is required and must not be empty.");

    public static LifecycleDomainException InvalidStateTransition(LifecycleStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static LifecycleDomainException AlreadyRunning(LifecycleId id)
        => new($"Lifecycle '{id.Value}' is already running.");

    public static LifecycleDomainException AlreadyCompleted(LifecycleId id)
        => new($"Lifecycle '{id.Value}' has already completed.");

    public static LifecycleDomainException AlreadyTerminated(LifecycleId id)
        => new($"Lifecycle '{id.Value}' has already been terminated.");
}

public sealed class LifecycleDomainException : Exception
{
    public LifecycleDomainException(string message) : base(message) { }
}
