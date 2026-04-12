namespace Whycespace.Domain.BusinessSystem.Execution.Sourcing;

public static class SourcingErrors
{
    public static SourcingDomainException MissingId()
        => new("SourcingId is required and must not be empty.");
    public static SourcingDomainException MissingRequestId()
        => new("SourcingRequestId is required and must not be empty.");
    public static SourcingDomainException AlreadySourced()
        => new("Sourcing has already been completed.");
    public static SourcingDomainException AlreadyFailed()
        => new("Sourcing has already failed.");
    public static SourcingDomainException InvalidStateTransition(SourcingStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}

public sealed class SourcingDomainException : Exception
{
    public SourcingDomainException(string message) : base(message) { }
}
