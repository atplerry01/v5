namespace Whycespace.Projections.Intelligence.Relationship.Graph;

public interface IGraphViewRepository
{
    Task SaveAsync(GraphReadModel model, CancellationToken ct = default);
    Task<GraphReadModel?> GetAsync(string id, CancellationToken ct = default);
}
