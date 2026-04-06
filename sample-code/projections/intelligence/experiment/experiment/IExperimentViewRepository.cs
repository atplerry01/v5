namespace Whycespace.Projections.Intelligence.Experiment.Experiment;

public interface IExperimentViewRepository
{
    Task SaveAsync(ExperimentReadModel model, CancellationToken ct = default);
    Task<ExperimentReadModel?> GetAsync(string id, CancellationToken ct = default);
}
