using Whycespace.Shared.Primitives.Id;
namespace Whycespace.Domain.BusinessSystem.Notification.Subscription;

public readonly record struct SubscriptionId(Guid Value)
{
    public static SubscriptionId FromSeed(string seed) => new(DeterministicIdHelper.FromSeed(seed));
    public static readonly SubscriptionId Empty = new(Guid.Empty);

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(SubscriptionId id) => id.Value;
    public static implicit operator SubscriptionId(Guid id) => new(id);
}
