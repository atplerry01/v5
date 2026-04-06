namespace Whycespace.Projections.Intelligence.Observability.Metric;

public interface IMetricViewRepository
{
    Task SaveAsync(MetricReadModel model, CancellationToken ct = default);
    Task<MetricReadModel?> GetAsync(string id, CancellationToken ct = default);
}
