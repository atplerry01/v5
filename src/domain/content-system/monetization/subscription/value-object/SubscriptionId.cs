namespace Whycespace.Domain.ContentSystem.Monetization.Subscription;

public readonly record struct SubscriptionId(Guid Value)
{
    public static SubscriptionId From(Guid value) => new(value);
    public override string ToString() => Value.ToString("D");
}
