namespace Whycespace.Domain.BusinessSystem.Notification.Delivery;

public static class DeliveryErrors
{
    public const string AlreadySent = "DELIVERY_ALREADY_SENT";
    public const string AlreadyFailed = "DELIVERY_ALREADY_FAILED";
    public const string MaxRetriesExceeded = "DELIVERY_MAX_RETRIES_EXCEEDED";
    public const string InvalidTransition = "DELIVERY_INVALID_TRANSITION";
}
