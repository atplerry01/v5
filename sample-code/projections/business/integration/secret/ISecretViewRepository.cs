namespace Whycespace.Projections.Business.Integration.Secret;

public interface ISecretViewRepository
{
    Task SaveAsync(SecretReadModel model, CancellationToken ct = default);
    Task<SecretReadModel?> GetAsync(string id, CancellationToken ct = default);
}
