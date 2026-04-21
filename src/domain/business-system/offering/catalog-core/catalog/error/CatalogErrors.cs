using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Offering.CatalogCore.Catalog;

public static class CatalogErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("CatalogId is required and must not be empty.");

    public static DomainException MissingStructure()
        => new DomainInvariantViolationException("Catalog must have a structure definition.");

    public static DomainException InvalidStateTransition(CatalogStatus currentStatus, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static DomainException MemberAlreadyPresent(CatalogMember member)
        => new DomainInvariantViolationException($"Catalog already contains member '{member.Kind}:{member.MemberId}'.");

    public static DomainException MemberNotPresent(CatalogMember member)
        => new DomainInvariantViolationException($"Catalog does not contain member '{member.Kind}:{member.MemberId}'.");

    public static DomainException ArchivedImmutable(CatalogId id)
        => new DomainInvariantViolationException($"Catalog '{id.Value}' is archived and cannot be mutated.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("Catalog has already been initialized.");
}
