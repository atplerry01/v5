namespace Whycespace.Projections.Orchestration.Workflow.Queue;

public interface IQueueViewRepository
{
    Task SaveAsync(QueueReadModel model, CancellationToken ct = default);
    Task<QueueReadModel?> GetAsync(string id, CancellationToken ct = default);
}
