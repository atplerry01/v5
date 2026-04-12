namespace Whycespace.Domain.BusinessSystem.Integration.Endpoint;

public static class EndpointErrors
{
    public static EndpointDomainException MissingId()
        => new("EndpointId is required and must not be empty.");

    public static EndpointDomainException MissingDefinition()
        => new("EndpointDefinition is required and must not be null.");

    public static EndpointDomainException InvalidStateTransition(EndpointStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static EndpointDomainException AlreadyAvailable(EndpointId id)
        => new($"Endpoint '{id.Value}' is already available.");

    public static EndpointDomainException AlreadyUnavailable(EndpointId id)
        => new($"Endpoint '{id.Value}' is already unavailable.");
}

public sealed class EndpointDomainException : Exception
{
    public EndpointDomainException(string message) : base(message) { }
}
