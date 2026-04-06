namespace Whycespace.Projections.Core.State.StateVersion;

public interface IStateVersionViewRepository
{
    Task SaveAsync(StateVersionReadModel model, CancellationToken ct = default);
    Task<StateVersionReadModel?> GetAsync(string id, CancellationToken ct = default);
}
