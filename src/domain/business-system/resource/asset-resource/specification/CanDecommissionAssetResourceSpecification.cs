namespace Whycespace.Domain.BusinessSystem.Resource.AssetResource;

public sealed class CanDecommissionAssetResourceSpecification
{
    public bool IsSatisfiedBy(AssetResourceStatus status)
    {
        return status == AssetResourceStatus.Active;
    }
}
