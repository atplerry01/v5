namespace Whycespace.Projections.Core.State.StateSnapshot;

public interface IStateSnapshotViewRepository
{
    Task SaveAsync(StateSnapshotReadModel model, CancellationToken ct = default);
    Task<StateSnapshotReadModel?> GetAsync(string id, CancellationToken ct = default);
}
