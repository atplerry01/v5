namespace Whycespace.Projections.Economic.Capital.Vault;

public interface IVaultViewRepository
{
    Task SaveAsync(VaultReadModel model, CancellationToken ct = default);
    Task<VaultReadModel?> GetAsync(string id, CancellationToken ct = default);
}
