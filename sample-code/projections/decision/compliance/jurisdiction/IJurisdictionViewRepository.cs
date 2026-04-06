namespace Whycespace.Projections.Decision.Compliance.Jurisdiction;

public interface IJurisdictionViewRepository
{
    Task SaveAsync(JurisdictionReadModel model, CancellationToken ct = default);
    Task<JurisdictionReadModel?> GetAsync(string id, CancellationToken ct = default);
}
