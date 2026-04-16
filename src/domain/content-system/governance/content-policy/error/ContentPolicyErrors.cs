using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Governance.ContentPolicy;

public static class ContentPolicyErrors
{
    public static DomainException InvalidName() => new("Content policy name must be non-empty.");
    public static DomainException InvalidBody() => new("Content policy body must be non-empty.");
    public static DomainException InvalidRevision() => new("Content policy revision number must be positive.");
    public static DomainException AlreadyPublished() => new("Content policy is already published.");
    public static DomainException AlreadyRetired() => new("Content policy is already retired.");
    public static DomainException CannotMutateRetired() => new("Retired content policies are immutable.");
    public static DomainException AmendmentNotIncrementing() =>
        new("Amendment revision number must be strictly greater than the current revision.");
    public static DomainInvariantViolationException NameMissing() =>
        new("Invariant violated: content policy must have a name.");
}
