namespace Whycespace.Projections.Intelligence.Experiment.Cohort;

public interface ICohortViewRepository
{
    Task SaveAsync(CohortReadModel model, CancellationToken ct = default);
    Task<CohortReadModel?> GetAsync(string id, CancellationToken ct = default);
}
