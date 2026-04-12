namespace Whycespace.Domain.BusinessSystem.Integration.Subscription;

public static class SubscriptionErrors
{
    public static SubscriptionDomainException MissingId()
        => new("SubscriptionId is required and must not be empty.");

    public static SubscriptionDomainException MissingTargetId()
        => new("SubscriptionTargetId is required and must not be empty.");

    public static SubscriptionDomainException AlreadyActive(SubscriptionId id)
        => new($"Subscription '{id.Value}' is already active.");

    public static SubscriptionDomainException AlreadyDeactivated(SubscriptionId id)
        => new($"Subscription '{id.Value}' is already deactivated.");

    public static SubscriptionDomainException InvalidStateTransition(SubscriptionStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}

public sealed class SubscriptionDomainException : Exception
{
    public SubscriptionDomainException(string message) : base(message) { }
}
