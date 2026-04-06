namespace Whycespace.Projections.Intelligence.Simulation.Outcome;

public interface IOutcomeViewRepository
{
    Task SaveAsync(OutcomeReadModel model, CancellationToken ct = default);
    Task<OutcomeReadModel?> GetAsync(string id, CancellationToken ct = default);
}
