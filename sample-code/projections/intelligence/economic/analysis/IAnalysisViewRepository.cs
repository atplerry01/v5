namespace Whycespace.Projections.Intelligence.Economic.Analysis;

public interface IAnalysisViewRepository
{
    Task SaveAsync(AnalysisReadModel model, CancellationToken ct = default);
    Task<AnalysisReadModel?> GetAsync(string id, CancellationToken ct = default);
}
