namespace Whycespace.Projections.Intelligence.Search.Query;

public interface IQueryViewRepository
{
    Task SaveAsync(QueryReadModel model, CancellationToken ct = default);
    Task<QueryReadModel?> GetAsync(string id, CancellationToken ct = default);
}
