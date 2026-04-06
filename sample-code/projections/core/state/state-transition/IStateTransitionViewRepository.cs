namespace Whycespace.Projections.Core.State.StateTransition;

public interface IStateTransitionViewRepository
{
    Task SaveAsync(StateTransitionReadModel model, CancellationToken ct = default);
    Task<StateTransitionReadModel?> GetAsync(string id, CancellationToken ct = default);
}
