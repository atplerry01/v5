using Whycespace.Shared.Primitives.Id;
namespace Whycespace.Domain.EconomicSystem.Enforcement.Enforcement;

public sealed record EnforcementId
{
    public Guid Value { get; }

    public EnforcementId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("EnforcementId cannot be empty.", nameof(value));

        Value = value;
    }

    public static EnforcementId FromSeed(string seed) => new(DeterministicIdHelper.FromSeed(seed));

    public override string ToString() => Value.ToString();
}
