namespace Whycespace.Domain.BusinessSystem.Resource.Workspace;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(WorkspaceStatus status)
    {
        return status == WorkspaceStatus.Provisioned;
    }
}
