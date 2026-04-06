namespace Whycespace.Projections.Intelligence.Experiment.ResultAnalysis;

public interface IResultAnalysisViewRepository
{
    Task SaveAsync(ResultAnalysisReadModel model, CancellationToken ct = default);
    Task<ResultAnalysisReadModel?> GetAsync(string id, CancellationToken ct = default);
}
