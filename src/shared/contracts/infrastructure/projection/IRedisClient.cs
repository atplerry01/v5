namespace Whyce.Shared.Contracts.Infrastructure.Projection;

public interface IRedisClient
{
    Task SetAsync<T>(string key, T value);
    Task<T?> GetAsync<T>(string key);
}
