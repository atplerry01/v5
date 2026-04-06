using Whycespace.Shared.Primitives.Id;
namespace Whycespace.Domain.TrustSystem.Identity.Device;

public sealed record DeviceId
{
    public Guid Value { get; }

    public DeviceId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("DeviceId cannot be empty.", nameof(value));
        Value = value;
    }

    public static DeviceId FromSeed(string seed) => new(DeterministicIdHelper.FromSeed(seed));
    public override string ToString() => Value.ToString();
}
