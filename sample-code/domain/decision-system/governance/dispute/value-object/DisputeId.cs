using Whycespace.Shared.Primitives.Id;
namespace Whycespace.Domain.DecisionSystem.Governance.Dispute;

public readonly record struct DisputeId(Guid Value)
{
    public static DisputeId FromSeed(string seed) => new(DeterministicIdHelper.FromSeed(seed));
    public static readonly DisputeId Empty = new(Guid.Empty);

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(DisputeId id) => id.Value;
    public static implicit operator DisputeId(Guid id) => new(id);
}
