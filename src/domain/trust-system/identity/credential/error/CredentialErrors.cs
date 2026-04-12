namespace Whycespace.Domain.TrustSystem.Identity.Credential;

public static class CredentialErrors
{
    public static CredentialDomainException MissingId()
        => new("CredentialId is required and must not be empty.");

    public static CredentialDomainException MissingDescriptor()
        => new("CredentialDescriptor is required and must not be default.");

    public static InvalidOperationException InvalidStateTransition(CredentialStatus status, string action)
        => new($"Cannot perform '{action}' when credential status is '{status}'.");
}

public sealed class CredentialDomainException : Exception
{
    public CredentialDomainException(string message) : base(message) { }
}
