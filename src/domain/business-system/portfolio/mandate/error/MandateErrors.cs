namespace Whycespace.Domain.BusinessSystem.Portfolio.Mandate;

public static class MandateErrors
{
    public static MandateDomainException MissingId()
        => new("MandateId is required and must not be empty.");

    public static MandateDomainException InvalidStateTransition(MandateStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static MandateDomainException NameRequired()
        => new("Mandate must have a name.");

    public static MandateDomainException AlreadyRevoked()
        => new("Mandate has been revoked and cannot be modified.");
}

public sealed class MandateDomainException : Exception
{
    public MandateDomainException(string message) : base(message) { }
}
