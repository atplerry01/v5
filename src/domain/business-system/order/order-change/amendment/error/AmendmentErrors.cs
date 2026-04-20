namespace Whycespace.Domain.BusinessSystem.Order.OrderChange.Amendment;

public static class AmendmentErrors
{
    public static AmendmentDomainException MissingId()
        => new("AmendmentId is required and must not be empty.");

    public static AmendmentDomainException MissingOrderRef()
        => new("Amendment must reference an order.");

    public static AmendmentDomainException InvalidStateTransition(AmendmentStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static AmendmentDomainException AlreadyTerminal(AmendmentId id, AmendmentStatus status)
        => new($"Amendment '{id.Value}' is already terminal ({status}).");
}

public sealed class AmendmentDomainException : Exception
{
    public AmendmentDomainException(string message) : base(message) { }
}
