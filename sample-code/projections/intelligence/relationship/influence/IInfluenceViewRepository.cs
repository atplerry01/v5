namespace Whycespace.Projections.Intelligence.Relationship.Influence;

public interface IInfluenceViewRepository
{
    Task SaveAsync(InfluenceReadModel model, CancellationToken ct = default);
    Task<InfluenceReadModel?> GetAsync(string id, CancellationToken ct = default);
}
