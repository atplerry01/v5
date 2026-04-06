namespace Whycespace.Projections.Global.ThroughputTracking;

public interface IThroughputTrackingViewRepository
{
    Task SaveAsync(ThroughputTrackingReadModel model, CancellationToken ct = default);
    Task<ThroughputTrackingReadModel?> GetAsync(string id, CancellationToken ct = default);
    Task<IReadOnlyList<ThroughputTrackingReadModel>> GetAllAsync(CancellationToken ct = default);
}
