namespace Whycespace.Domain.BusinessSystem.Resource.Reservation;

public sealed class ReservationAggregate
{
    public static ReservationAggregate Create()
    {
        var aggregate = new ReservationAggregate();
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
