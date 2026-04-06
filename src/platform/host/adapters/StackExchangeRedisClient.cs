using System.Text.Json;
using StackExchange.Redis;
using Whyce.Shared.Contracts.Infrastructure.Projection;

namespace Whyce.Platform.Host.Adapters;

/// <summary>
/// StackExchange.Redis-backed implementation of IRedisClient.
/// Uses JSON serialization for value storage.
/// </summary>
public sealed class StackExchangeRedisClient : IRedisClient
{
    private readonly IConnectionMultiplexer _multiplexer;

    public StackExchangeRedisClient(IConnectionMultiplexer multiplexer)
    {
        _multiplexer = multiplexer;
    }

    public async Task SetAsync<T>(string key, T value)
    {
        var db = _multiplexer.GetDatabase();
        var json = JsonSerializer.Serialize(value);
        await db.StringSetAsync(key, json);
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var db = _multiplexer.GetDatabase();
        var value = await db.StringGetAsync(key);
        if (value.IsNullOrEmpty)
            return default;
        return JsonSerializer.Deserialize<T>((string)value!);
    }
}
