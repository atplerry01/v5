namespace Whycespace.Projections.Intelligence.Search.Index;

public interface IIndexViewRepository
{
    Task SaveAsync(IndexReadModel model, CancellationToken ct = default);
    Task<IndexReadModel?> GetAsync(string id, CancellationToken ct = default);
}
