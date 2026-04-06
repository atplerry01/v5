namespace Whycespace.Projections.Intelligence.Simulation.Scenario;

public interface IScenarioViewRepository
{
    Task SaveAsync(ScenarioReadModel model, CancellationToken ct = default);
    Task<ScenarioReadModel?> GetAsync(string id, CancellationToken ct = default);
}
