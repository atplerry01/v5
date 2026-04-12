namespace Whycespace.Domain.BusinessSystem.Document.Version;

public sealed class CanReleaseSpecification
{
    public bool IsSatisfiedBy(VersionStatus status)
    {
        return status == VersionStatus.Draft;
    }
}
