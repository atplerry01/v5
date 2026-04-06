namespace Whycespace.Projections.Orchestration.Workflow.Checkpoint;

public interface ICheckpointViewRepository
{
    Task SaveAsync(CheckpointReadModel model, CancellationToken ct = default);
    Task<CheckpointReadModel?> GetAsync(string id, CancellationToken ct = default);
}
