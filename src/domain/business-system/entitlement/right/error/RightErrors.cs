namespace Whycespace.Domain.BusinessSystem.Entitlement.Right;

public static class RightErrors
{
    public static RightDomainException MissingId()
        => new("RightId is required and must not be empty.");

    public static RightDomainException MissingDefinition()
        => new("RightDefinition is required and must not be null.");

    public static RightDomainException InvalidStateTransition(RightStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static RightDomainException AlreadyActive(RightId id)
        => new($"Right '{id.Value}' is already active.");

    public static RightDomainException AlreadyDeprecated(RightId id)
        => new($"Right '{id.Value}' has already been deprecated.");
}

public sealed class RightDomainException : Exception
{
    public RightDomainException(string message) : base(message) { }
}
