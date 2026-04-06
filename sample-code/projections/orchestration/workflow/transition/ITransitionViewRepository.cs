namespace Whycespace.Projections.Orchestration.Workflow.Transition;

public interface ITransitionViewRepository
{
    Task SaveAsync(TransitionReadModel model, CancellationToken ct = default);
    Task<TransitionReadModel?> GetAsync(string id, CancellationToken ct = default);
}
