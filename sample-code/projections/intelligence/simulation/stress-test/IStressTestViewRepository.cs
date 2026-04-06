namespace Whycespace.Projections.Intelligence.Simulation.StressTest;

public interface IStressTestViewRepository
{
    Task SaveAsync(StressTestReadModel model, CancellationToken ct = default);
    Task<StressTestReadModel?> GetAsync(string id, CancellationToken ct = default);
}
