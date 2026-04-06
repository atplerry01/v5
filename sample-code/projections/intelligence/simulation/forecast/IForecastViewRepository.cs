namespace Whycespace.Projections.Intelligence.Simulation.Forecast;

public interface IForecastViewRepository
{
    Task SaveAsync(ForecastReadModel model, CancellationToken ct = default);
    Task<ForecastReadModel?> GetAsync(string id, CancellationToken ct = default);
}
