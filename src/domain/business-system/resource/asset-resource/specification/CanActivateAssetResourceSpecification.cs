namespace Whycespace.Domain.BusinessSystem.Resource.AssetResource;

public sealed class CanActivateAssetResourceSpecification
{
    public bool IsSatisfiedBy(AssetResourceStatus status)
    {
        return status == AssetResourceStatus.Pending;
    }
}
