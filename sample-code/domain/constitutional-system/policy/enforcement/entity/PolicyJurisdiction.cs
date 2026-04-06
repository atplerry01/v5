using Whycespace.Shared.Primitives.Id;

namespace Whycespace.Domain.ConstitutionalSystem.Policy.Registry;

public sealed class PolicyJurisdiction
{
    public Guid JurisdictionId { get; }
    public string Scope { get; }
    public Guid? ParentJurisdictionId { get; }
    public IReadOnlyList<Guid> ApplicablePolicyIds { get; }

    private PolicyJurisdiction(Guid id, string scope, Guid? parentId, IReadOnlyList<Guid> policyIds)
    {
        JurisdictionId = id;
        Scope = scope;
        ParentJurisdictionId = parentId;
        ApplicablePolicyIds = policyIds;
    }

    public static PolicyJurisdiction Create(string scope, Guid? parentId = null, IReadOnlyList<Guid>? policyIds = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scope);
        return new PolicyJurisdiction(DeterministicIdHelper.FromSeed($"PolicyJurisdiction:{scope}:{parentId}"), scope, parentId, policyIds ?? []);
    }

    public bool IsGlobal => Scope == "global";
    public bool HasParent => ParentJurisdictionId.HasValue;
}
