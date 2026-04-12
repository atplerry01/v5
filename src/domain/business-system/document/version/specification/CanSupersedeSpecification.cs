namespace Whycespace.Domain.BusinessSystem.Document.Version;

public sealed class CanSupersedeSpecification
{
    public bool IsSatisfiedBy(VersionStatus status)
    {
        return status == VersionStatus.Released;
    }
}
