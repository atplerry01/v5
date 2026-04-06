using Whycespace.Shared.Primitives.Id;
namespace Whycespace.Shared.Identity;

/// <summary>
/// Strongly typed, immutable, globally unique identity reference.
/// Used across all bounded contexts to reference an identity without
/// leaking identity domain logic.
/// </summary>
public sealed record IdentityId
{
    public Guid Value { get; }

    public IdentityId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("IdentityId cannot be empty.", nameof(value));

        Value = value;
    }

    public static IdentityId FromSeed(string seed) => new(DeterministicIdHelper.FromSeed(seed));

    public override string ToString() => Value.ToString();
}