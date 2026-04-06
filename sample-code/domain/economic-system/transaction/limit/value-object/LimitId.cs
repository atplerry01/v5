using Whycespace.Shared.Primitives.Id;
namespace Whycespace.Domain.EconomicSystem.Transaction.Limit;

public sealed record LimitId
{
    public Guid Value { get; }

    public LimitId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("LimitId cannot be empty.", nameof(value));

        Value = value;
    }

    public static LimitId FromSeed(string seed) => new(DeterministicIdHelper.FromSeed(seed));

    public override string ToString() => Value.ToString();
}
