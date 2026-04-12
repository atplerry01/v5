namespace Whycespace.Domain.BusinessSystem.Integration.Failure;

public static class FailureErrors
{
    public static FailureDomainException MissingId()
        => new("FailureId is required and must not be empty.");

    public static FailureDomainException MissingTypeId()
        => new("FailureTypeId is required and must not be empty.");

    public static FailureDomainException InvalidStateTransition(FailureStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static FailureDomainException AlreadyClassified(FailureId id)
        => new($"Failure '{id.Value}' has already been classified.");

    public static FailureDomainException AlreadyResolved(FailureId id)
        => new($"Failure '{id.Value}' has already been resolved.");
}

public sealed class FailureDomainException : Exception
{
    public FailureDomainException(string message) : base(message) { }
}
