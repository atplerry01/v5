using Whycespace.Shared.Primitives.Id;
namespace Whycespace.Domain.EconomicSystem.Capital.Binding;

public sealed record BindingId
{
    public Guid Value { get; }

    public BindingId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("BindingId cannot be empty.", nameof(value));

        Value = value;
    }

    public static BindingId FromSeed(string seed) => new(DeterministicIdHelper.FromSeed(seed));

    public override string ToString() => Value.ToString();
}
