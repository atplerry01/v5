namespace Whycespace.Projections.Decision.Compliance.ComplianceCase;

public interface IComplianceCaseViewRepository
{
    Task SaveAsync(ComplianceCaseReadModel model, CancellationToken ct = default);
    Task<ComplianceCaseReadModel?> GetAsync(string id, CancellationToken ct = default);
}
