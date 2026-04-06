namespace Whycespace.Projections.Business.Integration.Replay;

public interface IReplayViewRepository
{
    Task SaveAsync(ReplayReadModel model, CancellationToken ct = default);
    Task<ReplayReadModel?> GetAsync(string id, CancellationToken ct = default);
}
