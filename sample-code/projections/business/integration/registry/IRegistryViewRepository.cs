namespace Whycespace.Projections.Business.Integration.Registry;

public interface IRegistryViewRepository
{
    Task SaveAsync(RegistryReadModel model, CancellationToken ct = default);
    Task<RegistryReadModel?> GetAsync(string id, CancellationToken ct = default);
}
