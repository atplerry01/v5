namespace Whycespace.Projections.Intelligence.Simulation.Recommendation;

public interface IRecommendationViewRepository
{
    Task SaveAsync(RecommendationReadModel model, CancellationToken ct = default);
    Task<RecommendationReadModel?> GetAsync(string id, CancellationToken ct = default);
}
