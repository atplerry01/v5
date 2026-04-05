namespace Whycespace.Domain.BusinessSystem.Resource.Facility;

public sealed class FacilityAggregate
{
    public static FacilityAggregate Create()
    {
        var aggregate = new FacilityAggregate();
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
