namespace Whycespace.Projections.Business.Resource.Utilization;

public interface IUtilizationViewRepository
{
    Task SaveAsync(UtilizationReadModel model, CancellationToken ct = default);
    Task<UtilizationReadModel?> GetAsync(string id, CancellationToken ct = default);
}
