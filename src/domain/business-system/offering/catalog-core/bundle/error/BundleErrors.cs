using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Offering.CatalogCore.Bundle;

public static class BundleErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("BundleId is required and must not be empty.");

    public static DomainException MissingName()
        => new DomainInvariantViolationException("BundleName is required and must not be empty.");

    public static DomainException InvalidStateTransition(BundleStatus currentStatus, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static DomainException ArchivedImmutable(BundleId id)
        => new DomainInvariantViolationException($"Bundle '{id.Value}' is archived and cannot be mutated.");

    public static DomainException MemberAlreadyPresent(BundleMember member)
        => new DomainInvariantViolationException($"Bundle already contains member '{member.Kind}:{member.MemberId}'.");

    public static DomainException MemberNotPresent(BundleMember member)
        => new DomainInvariantViolationException($"Bundle does not contain member '{member.Kind}:{member.MemberId}'.");

    public static DomainException ActivationRequiresMembers()
        => new DomainInvariantViolationException("Bundle requires at least one member before activation.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("Bundle has already been initialized.");
}
