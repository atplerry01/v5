namespace Whycespace.Domain.BusinessSystem.Entitlement.UsageControl.UsageRight;

// Intentional: starts Available on creation. A usage-right only exists when
// units are allocated to use; there is no drafted/pre-allocation phase.
public enum UsageRightStatus
{
    Available,
    InUse,
    Consumed
}
