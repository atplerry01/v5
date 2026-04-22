using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.EntitlementHook;

public static class EntitlementHookErrors
{
    public static DomainException InvalidQueryResult()
        => new("Entitlement query result must be Entitled, NotEntitled, or Expired.");

    public static DomainException EmptyFailureReason()
        => new("Entitlement failure reason cannot be empty.");

    public static DomainInvariantViolationException MissingHookId()
        => new("EntitlementHook requires a valid HookId.");

    public static DomainInvariantViolationException MissingTargetRef()
        => new("EntitlementHook requires a valid TargetRef.");
}
