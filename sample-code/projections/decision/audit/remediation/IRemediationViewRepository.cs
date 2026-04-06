namespace Whycespace.Projections.Decision.Audit.Remediation;

public interface IRemediationViewRepository
{
    Task SaveAsync(RemediationReadModel model, CancellationToken ct = default);
    Task<RemediationReadModel?> GetAsync(string id, CancellationToken ct = default);
}
