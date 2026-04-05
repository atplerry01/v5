using System.Diagnostics;
using Npgsql;
using Whyce.Shared.Contracts.Infrastructure.Health;

namespace Whyce.Platform.Host.Health;

public sealed class PostgreSqlHealthCheck : IHealthCheck
{
    private readonly string _connectionString;

    public string Name => "postgres";

    public PostgreSqlHealthCheck(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<HealthCheckResult> CheckAsync()
    {
        var sw = Stopwatch.StartNew();
        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new NpgsqlCommand("SELECT 1", connection);
            await command.ExecuteScalarAsync();

            sw.Stop();
            return new HealthCheckResult(Name, true, "HEALTHY", sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return new HealthCheckResult(Name, false, "DOWN", sw.ElapsedMilliseconds, ex.Message);
        }
    }
}
