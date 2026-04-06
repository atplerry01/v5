namespace Whycespace.Projections.Decision.Risk.Rating;

public interface IRatingViewRepository
{
    Task SaveAsync(RatingReadModel model, CancellationToken ct = default);
    Task<RatingReadModel?> GetAsync(string id, CancellationToken ct = default);
}
