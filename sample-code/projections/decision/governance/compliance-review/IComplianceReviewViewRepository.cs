namespace Whycespace.Projections.Decision.Governance.ComplianceReview;

public interface IComplianceReviewViewRepository
{
    Task SaveAsync(ComplianceReviewReadModel model, CancellationToken ct = default);
    Task<ComplianceReviewReadModel?> GetAsync(string id, CancellationToken ct = default);
}
