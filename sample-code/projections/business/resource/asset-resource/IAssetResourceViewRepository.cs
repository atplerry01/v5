namespace Whycespace.Projections.Business.Resource.AssetResource;

public interface IAssetResourceViewRepository
{
    Task SaveAsync(AssetResourceReadModel model, CancellationToken ct = default);
    Task<AssetResourceReadModel?> GetAsync(string id, CancellationToken ct = default);
}
