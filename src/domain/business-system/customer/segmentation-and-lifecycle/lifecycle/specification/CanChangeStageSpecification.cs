namespace Whycespace.Domain.BusinessSystem.Customer.SegmentationAndLifecycle.Lifecycle;

// Canonical progression enforced:
//   Prospect  -> Onboarded
//   Onboarded -> Active
//   Active    -> Dormant | Churned
//   Dormant   -> Active  (revive) | Churned
//   Churned   terminal (no transitions out)
public sealed class CanChangeStageSpecification
{
    public bool IsSatisfiedBy(LifecycleStatus status, LifecycleStage from, LifecycleStage to)
    {
        if (status != LifecycleStatus.Tracking) return false;
        if (from == to) return false;

        return (from, to) switch
        {
            (LifecycleStage.Prospect,  LifecycleStage.Onboarded) => true,
            (LifecycleStage.Onboarded, LifecycleStage.Active)    => true,
            (LifecycleStage.Active,    LifecycleStage.Dormant)   => true,
            (LifecycleStage.Active,    LifecycleStage.Churned)   => true,
            (LifecycleStage.Dormant,   LifecycleStage.Active)    => true,
            (LifecycleStage.Dormant,   LifecycleStage.Churned)   => true,
            _ => false
        };
    }
}
