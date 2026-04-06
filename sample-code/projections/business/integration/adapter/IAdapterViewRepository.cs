namespace Whycespace.Projections.Business.Integration.Adapter;

public interface IAdapterViewRepository
{
    Task SaveAsync(AdapterReadModel model, CancellationToken ct = default);
    Task<AdapterReadModel?> GetAsync(string id, CancellationToken ct = default);
}
