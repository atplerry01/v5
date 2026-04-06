namespace Whycespace.Projections.Intelligence.Capacity.AllocationForecast;

public interface IAllocationForecastViewRepository
{
    Task SaveAsync(AllocationForecastReadModel model, CancellationToken ct = default);
    Task<AllocationForecastReadModel?> GetAsync(string id, CancellationToken ct = default);
}
