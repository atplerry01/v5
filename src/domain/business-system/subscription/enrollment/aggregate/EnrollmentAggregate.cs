namespace Whycespace.Domain.BusinessSystem.Subscription.Enrollment;

public sealed class EnrollmentAggregate
{
    public static EnrollmentAggregate Create()
    {
        var aggregate = new EnrollmentAggregate();
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
