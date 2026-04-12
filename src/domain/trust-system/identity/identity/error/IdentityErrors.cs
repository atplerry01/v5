namespace Whycespace.Domain.TrustSystem.Identity.Identity;

public static class IdentityErrors
{
    public static IdentityDomainException MissingId()
        => new("IdentityId is required and must not be empty.");

    public static IdentityDomainException MissingDescriptor()
        => new("IdentityDescriptor is required and must not be default.");

    public static IdentityDomainException InvalidStateTransition(IdentityStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}

public sealed class IdentityDomainException : Exception
{
    public IdentityDomainException(string message) : base(message) { }
}
