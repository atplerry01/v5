using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Offering.CommercialShape.Package;

public static class PackageErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("PackageId is required and must not be empty.");

    public static DomainException MissingCode()
        => new DomainInvariantViolationException("PackageCode is required and must not be empty.");

    public static DomainException MissingName()
        => new DomainInvariantViolationException("PackageName is required and must not be empty.");

    public static DomainException InvalidStateTransition(PackageStatus currentStatus, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static DomainException ArchivedImmutable(PackageId id)
        => new DomainInvariantViolationException($"Package '{id.Value}' is archived and cannot be mutated.");

    public static DomainException MemberAlreadyPresent(PackageMember member)
        => new DomainInvariantViolationException($"Package already contains member '{member.Kind}:{member.MemberId}'.");

    public static DomainException MemberNotPresent(PackageMember member)
        => new DomainInvariantViolationException($"Package does not contain member '{member.Kind}:{member.MemberId}'.");

    public static DomainException ActivationRequiresMembers()
        => new DomainInvariantViolationException("Package requires at least one member before activation.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("Package has already been initialized.");
}
