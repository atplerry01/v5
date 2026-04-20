using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.CoreObject.Asset;

public static class AssetErrors
{
    public static DomainException AssetAlreadyRetired()
        => new("Asset is already retired.");

    public static DomainException AssetNotRetired()
        => new("Asset is not retired.");

    public static DomainException AssetAlreadyActive()
        => new("Asset is already active.");

    public static DomainException CannotModifyRetiredAsset()
        => new("Cannot modify a retired asset.");

    public static DomainInvariantViolationException MissingClassification()
        => new("Active asset must carry a classification.");

    public static DomainException CannotAssignKindToRetiredAsset()
        => new("Cannot assign kind to a retired asset.");

    public static DomainException AssetKindUnchanged()
        => new("Asset kind is unchanged.");
}
