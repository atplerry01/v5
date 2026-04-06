namespace Whycespace.Projections.Core.Temporal.TemporalState;

public interface ITemporalStateViewRepository
{
    Task SaveAsync(TemporalStateReadModel model, CancellationToken ct = default);
    Task<TemporalStateReadModel?> GetAsync(string id, CancellationToken ct = default);
}
