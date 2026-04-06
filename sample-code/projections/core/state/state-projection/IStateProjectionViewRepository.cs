namespace Whycespace.Projections.Core.State.StateProjection;

public interface IStateProjectionViewRepository
{
    Task SaveAsync(StateProjectionReadModel model, CancellationToken ct = default);
    Task<StateProjectionReadModel?> GetAsync(string id, CancellationToken ct = default);
}
