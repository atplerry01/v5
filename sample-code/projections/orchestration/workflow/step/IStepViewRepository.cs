namespace Whycespace.Projections.Orchestration.Workflow.Step;

public interface IStepViewRepository
{
    Task SaveAsync(StepReadModel model, CancellationToken ct = default);
    Task<StepReadModel?> GetAsync(string id, CancellationToken ct = default);
}
