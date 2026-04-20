using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.ContentArtifact.Asset;

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
}
