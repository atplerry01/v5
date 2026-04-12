namespace Whycespace.Domain.BusinessSystem.Integration.Delivery;

public static class DeliveryErrors
{
    public static ArgumentException MissingId()
        => new("DeliveryId is required and must not be empty.");

    public static ArgumentException MissingDescriptor()
        => new("DeliveryDescriptor is required.");

    public static InvalidOperationException InvalidStateTransition(DeliveryStatus currentStatus, string action)
        => new($"Cannot {action} when delivery status is {currentStatus}.");
}
