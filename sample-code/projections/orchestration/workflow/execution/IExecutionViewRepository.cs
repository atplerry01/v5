namespace Whycespace.Projections.Orchestration.Workflow.Execution;

public interface IExecutionViewRepository
{
    Task SaveAsync(ExecutionReadModel model, CancellationToken ct = default);
    Task<ExecutionReadModel?> GetAsync(string id, CancellationToken ct = default);
}
