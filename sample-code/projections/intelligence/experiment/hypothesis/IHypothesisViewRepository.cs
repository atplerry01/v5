namespace Whycespace.Projections.Intelligence.Experiment.Hypothesis;

public interface IHypothesisViewRepository
{
    Task SaveAsync(HypothesisReadModel model, CancellationToken ct = default);
    Task<HypothesisReadModel?> GetAsync(string id, CancellationToken ct = default);
}
