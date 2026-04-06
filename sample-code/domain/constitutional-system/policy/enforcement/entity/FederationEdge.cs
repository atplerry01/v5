namespace Whycespace.Domain.ConstitutionalSystem.Policy.Registry;

using Whycespace.Domain.SharedKernel;
using Whycespace.Shared.Primitives.Id;

public sealed class FederationEdge : Entity
{
    private FederationEdge() { }

    public Guid SourcePolicyId { get; private set; }
    public Guid TargetPolicyId { get; private set; }
    public FederationRelationType RelationType { get; private set; } = default!;

    public static FederationEdge Create(
        Guid sourcePolicyId,
        Guid targetPolicyId,
        FederationRelationType relationType)
    {
        ArgumentNullException.ThrowIfNull(relationType);

        if (sourcePolicyId == targetPolicyId)
            throw new ArgumentException("Self-referencing edges are not allowed.");

        return new FederationEdge
        {
            Id = DeterministicIdHelper.FromSeed($"FederationEdge:{sourcePolicyId}:{targetPolicyId}:{relationType}"),
            SourcePolicyId = sourcePolicyId,
            TargetPolicyId = targetPolicyId,
            RelationType = relationType
        };
    }
}
