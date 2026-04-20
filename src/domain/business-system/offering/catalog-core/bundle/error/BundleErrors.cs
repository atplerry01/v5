namespace Whycespace.Domain.BusinessSystem.Offering.CatalogCore.Bundle;

public static class BundleErrors
{
    public static BundleDomainException MissingId()
        => new("BundleId is required and must not be empty.");

    public static BundleDomainException InvalidStateTransition(BundleStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static BundleDomainException ArchivedImmutable(BundleId id)
        => new($"Bundle '{id.Value}' is archived and cannot be mutated.");

    public static BundleDomainException MemberAlreadyPresent(BundleMember member)
        => new($"Bundle already contains member '{member.Kind}:{member.MemberId}'.");

    public static BundleDomainException MemberNotPresent(BundleMember member)
        => new($"Bundle does not contain member '{member.Kind}:{member.MemberId}'.");

    public static BundleDomainException ActivationRequiresMembers()
        => new("Bundle requires at least one member before activation.");
}

public sealed class BundleDomainException : Exception
{
    public BundleDomainException(string message) : base(message) { }
}
