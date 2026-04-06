using Whycespace.Shared.Primitives.Id;
namespace Whycespace.Domain.DecisionSystem.Governance.Scope;

public readonly record struct ScopeId(Guid Value)
{
    public static ScopeId FromSeed(string seed) => new(DeterministicIdHelper.FromSeed(seed));
    public static readonly ScopeId Empty = new(Guid.Empty);

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(ScopeId id) => id.Value;
    public static implicit operator ScopeId(Guid id) => new(id);
}
