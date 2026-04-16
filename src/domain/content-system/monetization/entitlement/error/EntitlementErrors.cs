using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Monetization.Entitlement;

public static class EntitlementErrors
{
    public static DomainException InvalidHolder() => new("Entitlement holder reference must be non-empty.");
    public static DomainException InvalidScope() => new("Entitlement scope (resource + permission) must be non-empty.");
    public static DomainException InvalidValidity() => new("Entitlement validity end must be strictly after start.");
    public static DomainException AlreadyRevoked() => new("Entitlement is already revoked.");
    public static DomainException CannotExtendRevoked() => new("Revoked entitlement cannot be extended.");
    public static DomainException CannotDowngradeRevoked() => new("Revoked entitlement cannot be downgraded.");
    public static DomainInvariantViolationException HolderMissing() =>
        new("Invariant violated: entitlement must have a holder.");
}
