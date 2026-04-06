namespace Whycespace.Projections.Decision.Governance.Review;

public interface IReviewViewRepository
{
    Task SaveAsync(ReviewReadModel model, CancellationToken ct = default);
    Task<ReviewReadModel?> GetAsync(string id, CancellationToken ct = default);
}
