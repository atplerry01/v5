using Whycespace.Shared.Primitives.Id;

namespace Whycespace.Domain.EconomicSystem.Revenue.Payout;

public sealed record PayoutId
{
    public Guid Value { get; }

    public PayoutId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PayoutId cannot be empty.", nameof(value));

        Value = value;
    }

    public static PayoutId FromSeed(string seed) => new(DeterministicIdHelper.FromSeed(seed));

    public static implicit operator Guid(PayoutId id) => id.Value;

    public override string ToString() => Value.ToString();
}
