namespace Whycespace.Projections.Intelligence.Economic.Anomaly;

public interface IAnomalyViewRepository
{
    Task SaveAsync(AnomalyReadModel model, CancellationToken ct = default);
    Task<AnomalyReadModel?> GetAsync(string id, CancellationToken ct = default);
}
