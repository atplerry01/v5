namespace Whycespace.Domain.BusinessSystem.Notification.Subscription;

public static class SubscriptionErrors
{
    public static SubscriptionDomainException MissingId()
        => new("SubscriptionId is required and must not be empty.");

    public static SubscriptionDomainException InvalidTarget()
        => new("Subscription must define a valid target with reference and type.");

    public static SubscriptionDomainException InvalidStateTransition(SubscriptionStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}

public sealed class SubscriptionDomainException : Exception
{
    public SubscriptionDomainException(string message) : base(message) { }
}
