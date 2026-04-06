namespace Whycespace.Projections.Business.Resource.MaintenanceResource;

public interface IMaintenanceResourceViewRepository
{
    Task SaveAsync(MaintenanceResourceReadModel model, CancellationToken ct = default);
    Task<MaintenanceResourceReadModel?> GetAsync(string id, CancellationToken ct = default);
}
