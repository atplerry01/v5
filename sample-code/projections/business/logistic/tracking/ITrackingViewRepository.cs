namespace Whycespace.Projections.Business.Logistic.Tracking;

public interface ITrackingViewRepository
{
    Task SaveAsync(TrackingReadModel model, CancellationToken ct = default);
    Task<TrackingReadModel?> GetAsync(string id, CancellationToken ct = default);
}
