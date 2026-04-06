using Whycespace.Shared.Primitives.Id;
namespace Whycespace.Domain.DecisionSystem.Governance.ComplianceReview;

public readonly record struct AuditId(Guid Value)
{
    public static AuditId FromSeed(string seed) => new(DeterministicIdHelper.FromSeed(seed));
    public static readonly AuditId Empty = new(Guid.Empty);

    public bool IsEmpty => Value == Guid.Empty;

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(AuditId id) => id.Value;
    public static implicit operator AuditId(Guid id) => new(id);
}
