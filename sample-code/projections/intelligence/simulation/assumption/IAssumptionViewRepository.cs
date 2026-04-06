namespace Whycespace.Projections.Intelligence.Simulation.Assumption;

public interface IAssumptionViewRepository
{
    Task SaveAsync(AssumptionReadModel model, CancellationToken ct = default);
    Task<AssumptionReadModel?> GetAsync(string id, CancellationToken ct = default);
}
