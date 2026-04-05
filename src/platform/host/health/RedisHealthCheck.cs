using System.Diagnostics;
using StackExchange.Redis;
using Whyce.Shared.Contracts.Infrastructure.Health;

namespace Whyce.Platform.Host.Health;

public sealed class RedisHealthCheck : IHealthCheck
{
    private readonly string _connectionString;

    public string Name => "redis";

    public RedisHealthCheck(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<HealthCheckResult> CheckAsync()
    {
        var sw = Stopwatch.StartNew();
        try
        {
            using var connection = await ConnectionMultiplexer.ConnectAsync(_connectionString);
            var db = connection.GetDatabase();

            var testKey = "__health_check__";
            await db.StringSetAsync(testKey, "ok", TimeSpan.FromSeconds(5));
            var value = await db.StringGetAsync(testKey);
            await db.KeyDeleteAsync(testKey);

            sw.Stop();

            if (value != "ok")
            {
                return new HealthCheckResult(Name, false, "DEGRADED", sw.ElapsedMilliseconds, "Set/Get mismatch");
            }

            return new HealthCheckResult(Name, true, "HEALTHY", sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return new HealthCheckResult(Name, false, "DOWN", sw.ElapsedMilliseconds, ex.Message);
        }
    }
}
