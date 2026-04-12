namespace Whycespace.Domain.BusinessSystem.Notification.Delivery;

public static class DeliveryErrors
{
    public static DeliveryDomainException MissingId()
        => new("DeliveryId is required and must not be empty.");

    public static DeliveryDomainException InvalidContract()
        => new("Delivery must define a valid contract with channel reference.");

    public static DeliveryDomainException InvalidStateTransition(DeliveryStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}

public sealed class DeliveryDomainException : Exception
{
    public DeliveryDomainException(string message) : base(message) { }
}
