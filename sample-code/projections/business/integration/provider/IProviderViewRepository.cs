namespace Whycespace.Projections.Business.Integration.Provider;

public interface IProviderViewRepository
{
    Task SaveAsync(ProviderReadModel model, CancellationToken ct = default);
    Task<ProviderReadModel?> GetAsync(string id, CancellationToken ct = default);
}
