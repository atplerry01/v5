namespace Whycespace.Domain.BusinessSystem.Resource.Workspace;

public sealed class CanDecommissionSpecification
{
    public bool IsSatisfiedBy(WorkspaceStatus status)
    {
        return status == WorkspaceStatus.Active;
    }
}
