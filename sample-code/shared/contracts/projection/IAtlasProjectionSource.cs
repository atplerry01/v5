namespace Whycespace.Shared.Contracts.Projection;

/// <summary>
/// Atlas projection query contract — relocated from src/systems/midstream/whyceatlas/.
/// Projection contracts belong in the shared layer, not in systems.
/// </summary>
public interface IAtlasProjectionSource
{
    Task<T?> QueryAsync<T>(string projectionName, IReadOnlyDictionary<string, object> parameters, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> QueryListAsync<T>(string projectionName, IReadOnlyDictionary<string, object> parameters, CancellationToken cancellationToken = default);
}
