namespace Whycespace.Domain.BusinessSystem.Notification.Subscription;

public readonly record struct SubscriptionTarget
{
    public Guid TargetReference { get; }
    public string TargetType { get; }

    public SubscriptionTarget(Guid targetReference, string targetType)
    {
        if (targetReference == Guid.Empty)
            throw new ArgumentException("Target reference must not be empty.", nameof(targetReference));

        if (string.IsNullOrWhiteSpace(targetType))
            throw new ArgumentException("Target type must not be empty.", nameof(targetType));

        TargetReference = targetReference;
        TargetType = targetType;
    }
}
