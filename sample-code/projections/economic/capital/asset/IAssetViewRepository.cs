namespace Whycespace.Projections.Economic.Capital.Asset;

public interface IAssetViewRepository
{
    Task SaveAsync(AssetReadModel model, CancellationToken ct = default);
    Task<AssetReadModel?> GetAsync(string id, CancellationToken ct = default);
}
