namespace Whycespace.Domain.BusinessSystem.Integration.Client;

public static class ClientErrors
{
    public static ClientDomainException MissingId()
        => new("ClientId is required and must not be empty.");

    public static ClientDomainException MissingExternalId()
        => new("ExternalClientId is required and must not be empty.");

    public static ClientDomainException CredentialRequired()
        => new("Client must have at least one credential reference before activation.");

    public static ClientDomainException InvalidStateTransition(ClientStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static ClientDomainException AlreadyActive(ClientId id)
        => new($"Client '{id.Value}' is already active.");

    public static ClientDomainException AlreadySuspended(ClientId id)
        => new($"Client '{id.Value}' is already suspended.");
}

public sealed class ClientDomainException : Exception
{
    public ClientDomainException(string message) : base(message) { }
}
