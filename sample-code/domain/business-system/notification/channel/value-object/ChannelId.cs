using Whycespace.Shared.Primitives.Id;
namespace Whycespace.Domain.BusinessSystem.Notification.Channel;

public readonly record struct ChannelId(Guid Value)
{
    public static ChannelId FromSeed(string seed) => new(DeterministicIdHelper.FromSeed(seed));
    public static readonly ChannelId Empty = new(Guid.Empty);

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(ChannelId id) => id.Value;
    public static implicit operator ChannelId(Guid id) => new(id);
}
