namespace Whycespace.Shared.Contracts.Projection;

/// <summary>
/// Projection query contract for read-model access.
/// Implementations live in infrastructure adapters.
/// </summary>
public interface IProjectionQuerySource
{
    Task<T?> QueryAsync<T>(string projectionName, IReadOnlyDictionary<string, object> parameters, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> QueryListAsync<T>(string projectionName, IReadOnlyDictionary<string, object> parameters, CancellationToken cancellationToken = default);
}
