namespace Whycespace.Domain.BusinessSystem.Document.Version;

public sealed class IsImmutableSpecification
{
    public bool IsSatisfiedBy(VersionStatus status)
    {
        return status == VersionStatus.Superseded;
    }
}
