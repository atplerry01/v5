namespace Whycespace.Projections.Decision.Compliance.Attestation;

public interface IAttestationViewRepository
{
    Task SaveAsync(AttestationReadModel model, CancellationToken ct = default);
    Task<AttestationReadModel?> GetAsync(string id, CancellationToken ct = default);
}
