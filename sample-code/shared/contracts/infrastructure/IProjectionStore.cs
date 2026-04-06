namespace Whycespace.Shared.Contracts.Infrastructure;

public interface IProjectionStore
{
    Task<T?> GetAsync<T>(string projectionName, string key, CancellationToken cancellationToken = default) where T : class;
    Task SetAsync<T>(string projectionName, string key, T value, CancellationToken cancellationToken = default) where T : class;
    Task RemoveAsync(string projectionName, string key, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> GetAllAsync<T>(string projectionName, CancellationToken cancellationToken = default) where T : class;
    Task<long> GetCheckpointAsync(string projectionName, CancellationToken cancellationToken = default);
    Task SetCheckpointAsync(string projectionName, long position, CancellationToken cancellationToken = default);
}
