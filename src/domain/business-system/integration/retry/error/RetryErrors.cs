namespace Whycespace.Domain.BusinessSystem.Integration.Retry;

public static class RetryErrors
{
    public static RetryDomainException MissingId()
        => new("RetryId is required and must not be empty.");

    public static RetryDomainException MissingPolicyId()
        => new("RetryPolicyId is required and must not be empty.");

    public static RetryDomainException InvalidStateTransition(RetryStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static RetryDomainException AlreadyActive(RetryId id)
        => new($"Retry '{id.Value}' is already active.");

    public static RetryDomainException AlreadyDisabled(RetryId id)
        => new($"Retry '{id.Value}' is already disabled.");
}

public sealed class RetryDomainException : Exception
{
    public RetryDomainException(string message) : base(message) { }
}
