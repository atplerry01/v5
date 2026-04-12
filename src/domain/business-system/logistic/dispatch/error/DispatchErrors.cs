namespace Whycespace.Domain.BusinessSystem.Logistic.Dispatch;

public static class DispatchErrors
{
    public static DispatchDomainException MissingId()
        => new("DispatchId is required and must not be empty.");

    public static DispatchDomainException ShipmentReferenceRequired()
        => new("Dispatch must reference a shipment.");

    public static DispatchDomainException InvalidStateTransition(DispatchStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static DispatchDomainException AlreadyCompleted()
        => new("Dispatch has been completed and cannot be modified.");
}

public sealed class DispatchDomainException : Exception
{
    public DispatchDomainException(string message) : base(message) { }
}
