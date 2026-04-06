namespace Whycespace.Projections.Trust.Identity.Credential;

public interface ICredentialViewRepository
{
    Task SaveAsync(CredentialReadModel model, CancellationToken ct = default);
    Task<CredentialReadModel?> GetAsync(string id, CancellationToken ct = default);
}
