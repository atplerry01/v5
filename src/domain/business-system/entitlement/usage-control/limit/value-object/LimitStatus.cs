namespace Whycespace.Domain.BusinessSystem.Entitlement.UsageControl.Limit;

// Intentional: starts Defined on creation. Defining a limit IS the creation
// moment; Enforce activates monitoring; Breach is the terminal overrun state.
public enum LimitStatus
{
    Defined,
    Enforced,
    Breached
}
