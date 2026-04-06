using Whycespace.Shared.Primitives.Id;

namespace Whycespace.Domain.StructuralSystem.Cluster.Continuity;

public sealed record ContinuityPlanId(Guid Value)
{
    public static ContinuityPlanId FromSeed(string seed) => new(DeterministicIdHelper.FromSeed(seed));

    public static implicit operator Guid(ContinuityPlanId id) => id.Value;
}
