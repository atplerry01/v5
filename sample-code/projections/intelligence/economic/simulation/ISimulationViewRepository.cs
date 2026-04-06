namespace Whycespace.Projections.Intelligence.Economic.Simulation;

public interface ISimulationViewRepository
{
    Task SaveAsync(SimulationReadModel model, CancellationToken ct = default);
    Task<SimulationReadModel?> GetAsync(string id, CancellationToken ct = default);
}
