using Whycespace.Shared.Primitives.Id;
namespace Whycespace.Domain.BusinessSystem.Notification.Delivery;

public readonly record struct DeliveryId(Guid Value)
{
    public static DeliveryId FromSeed(string seed) => new(DeterministicIdHelper.FromSeed(seed));
    public static readonly DeliveryId Empty = new(Guid.Empty);

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(DeliveryId id) => id.Value;
    public static implicit operator DeliveryId(Guid id) => new(id);
}
