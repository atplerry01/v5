namespace Whycespace.Domain.BusinessSystem.Integration.Gateway;

public static class GatewayErrors
{
    public static GatewayDomainException MissingId()
        => new("GatewayId is required and must not be empty.");

    public static GatewayDomainException MissingRouteId()
        => new("GatewayRouteId is required and must not be empty.");

    public static GatewayDomainException InvalidStateTransition(GatewayStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static GatewayDomainException AlreadyActive(GatewayId id)
        => new($"Gateway '{id.Value}' is already active.");

    public static GatewayDomainException AlreadyDisabled(GatewayId id)
        => new($"Gateway '{id.Value}' is already disabled.");
}

public sealed class GatewayDomainException : Exception
{
    public GatewayDomainException(string message) : base(message) { }
}
