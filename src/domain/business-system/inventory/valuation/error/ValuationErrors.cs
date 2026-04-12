namespace Whycespace.Domain.BusinessSystem.Inventory.Valuation;

public static class ValuationErrors
{
    public static ValuationDomainException MissingId()
        => new("ValuationId is required and must not be empty.");

    public static ValuationDomainException InvalidMethod()
        => new("Valuation method must be a defined value.");

    public static ValuationDomainException AlreadyDeactivated(ValuationId id)
        => new($"Valuation policy '{id.Value}' has already been deactivated.");

    public static ValuationDomainException InvalidStateTransition(ValuationStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}

public sealed class ValuationDomainException : Exception
{
    public ValuationDomainException(string message) : base(message) { }
}
