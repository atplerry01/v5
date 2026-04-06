namespace Whycespace.Projections.Intelligence.Search.Result;

public interface IResultViewRepository
{
    Task SaveAsync(ResultReadModel model, CancellationToken ct = default);
    Task<ResultReadModel?> GetAsync(string id, CancellationToken ct = default);
}
