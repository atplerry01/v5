namespace Whycespace.Projections.Orchestration.Workflow.Instance;

public interface IInstanceViewRepository
{
    Task SaveAsync(InstanceReadModel model, CancellationToken ct = default);
    Task<InstanceReadModel?> GetAsync(string id, CancellationToken ct = default);
}
