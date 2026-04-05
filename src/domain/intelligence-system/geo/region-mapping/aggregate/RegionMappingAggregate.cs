namespace Whycespace.Domain.IntelligenceSystem.Geo.RegionMapping;

public sealed class RegionMappingAggregate
{
    public static RegionMappingAggregate Create()
    {
        var aggregate = new RegionMappingAggregate();
        aggregate.ValidateBeforeChange();
        aggregate.EnsureInvariants();
        // POLICY HOOK (to be enforced by runtime)
        return aggregate;
    }

    private void EnsureInvariants()
    {
        // Domain invariant checks enforced BEFORE any event is raised
    }

    private void ValidateBeforeChange()
    {
        // Pre-change validation gate
    }
}
