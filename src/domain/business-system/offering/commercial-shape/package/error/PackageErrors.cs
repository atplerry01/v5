namespace Whycespace.Domain.BusinessSystem.Offering.CommercialShape.Package;

public static class PackageErrors
{
    public static PackageDomainException MissingId()
        => new("PackageId is required and must not be empty.");

    public static PackageDomainException InvalidStateTransition(PackageStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static PackageDomainException ArchivedImmutable(PackageId id)
        => new($"Package '{id.Value}' is archived and cannot be mutated.");

    public static PackageDomainException MemberAlreadyPresent(PackageMember member)
        => new($"Package already contains member '{member.Kind}:{member.MemberId}'.");

    public static PackageDomainException MemberNotPresent(PackageMember member)
        => new($"Package does not contain member '{member.Kind}:{member.MemberId}'.");

    public static PackageDomainException ActivationRequiresMembers()
        => new("Package requires at least one member before activation.");
}

public sealed class PackageDomainException : Exception
{
    public PackageDomainException(string message) : base(message) { }
}
