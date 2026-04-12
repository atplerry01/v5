namespace Whycespace.Domain.BusinessSystem.Integration.Connector;

public static class ConnectorErrors
{
    public static ConnectorDomainException MissingId()
        => new("ConnectorId is required and must not be empty.");

    public static ConnectorDomainException MissingTargetId()
        => new("ConnectorTargetId is required and must not be empty.");

    public static ConnectorDomainException InvalidStateTransition(ConnectorStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static ConnectorDomainException AlreadyConnected(ConnectorId id)
        => new($"Connector '{id.Value}' is already connected.");

    public static ConnectorDomainException AlreadyDisconnected(ConnectorId id)
        => new($"Connector '{id.Value}' is already disconnected.");
}

public sealed class ConnectorDomainException : Exception
{
    public ConnectorDomainException(string message) : base(message) { }
}
