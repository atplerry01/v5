namespace Whycespace.Projections.Trust.Identity.Verification;

public interface IVerificationViewRepository
{
    Task SaveAsync(VerificationReadModel model, CancellationToken ct = default);
    Task<VerificationReadModel?> GetAsync(string id, CancellationToken ct = default);
}
