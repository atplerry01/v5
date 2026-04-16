using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Monetization.Subscription;

public static class SubscriptionErrors
{
    public static DomainException InvalidSubscriber() => new("Subscription subscriber reference must be non-empty.");
    public static DomainException InvalidPlan() => new("Subscription plan reference must be non-empty.");
    public static DomainException InvalidPeriod() => new("Period end must be strictly after period start.");
    public static DomainException AlreadyActive() => new("Subscription is already active.");
    public static DomainException NotActive() => new("Subscription is not active.");
    public static DomainException AlreadyCancelled() => new("Subscription is already cancelled.");
    public static DomainException AlreadyExpired() => new("Subscription is already expired.");
    public static DomainException CannotActivateFromStatus(SubscriptionStatus s) =>
        new($"Subscription cannot be activated from {s}.");
    public static DomainInvariantViolationException SubscriberMissing() =>
        new("Invariant violated: subscription must have a subscriber.");
}
