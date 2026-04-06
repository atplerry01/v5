namespace Whycespace.Projections.Business.Resource.Workspace;

public interface IWorkspaceViewRepository
{
    Task SaveAsync(WorkspaceReadModel model, CancellationToken ct = default);
    Task<WorkspaceReadModel?> GetAsync(string id, CancellationToken ct = default);
}
