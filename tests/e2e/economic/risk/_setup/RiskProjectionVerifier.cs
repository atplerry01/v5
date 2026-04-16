using Npgsql;

namespace Whycespace.Tests.E2E.Economic.Risk.Setup;

public static class RiskProjectionVerifier
{
    public static async Task<string> PollUntilPresentAsync(
        string schema, string table, Guid aggregateId, TimeSpan timeout, CancellationToken ct = default)
    {
        var deadline = DateTime.UtcNow + timeout;
        Exception? last = null;
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                var json = await TryLoadStateAsync(schema, table, aggregateId, ct);
                if (json is not null) return json;
            }
            catch (Exception ex) { last = ex; }
            await Task.Delay(200, ct);
        }
        throw new TimeoutException(
            $"Projection row not present within {timeout.TotalSeconds:F0}s for {schema}.{table} aggregate_id={aggregateId}." +
            (last is null ? "" : $" Last DB error: {last.Message}"));
    }

    public static async Task<string> PollUntilStatusAsync(
        string schema, string table, Guid aggregateId, int expectedStatus, TimeSpan timeout, CancellationToken ct = default)
    {
        var deadline = DateTime.UtcNow + timeout;
        string? lastObserved = null;
        while (DateTime.UtcNow < deadline)
        {
            var json = await TryLoadStateAsync(schema, table, aggregateId, ct);
            if (json is not null)
            {
                lastObserved = json;
                if (json.Contains($"\"Status\":{expectedStatus}", StringComparison.Ordinal) ||
                    json.Contains($"\"Status\": {expectedStatus}", StringComparison.Ordinal) ||
                    json.Contains($"\"status\":{expectedStatus}", StringComparison.Ordinal) ||
                    json.Contains($"\"status\": {expectedStatus}", StringComparison.Ordinal))
                    return json;
            }
            await Task.Delay(200, ct);
        }
        throw new TimeoutException(
            $"Projection row did not reach status={expectedStatus} within {timeout.TotalSeconds:F0}s for {schema}.{table} aggregate_id={aggregateId}. " +
            $"Last observed: {lastObserved ?? "<absent>"}.");
    }

    public static async Task AssertAbsentAsync(
        string schema, string table, Guid aggregateId, CancellationToken ct = default)
    {
        var json = await TryLoadStateAsync(schema, table, aggregateId, ct);
        if (json is not null)
            throw new Xunit.Sdk.XunitException(
                $"Expected no projection row for {schema}.{table} aggregate_id={aggregateId}, but one was present.");
    }

    private static async Task<string?> TryLoadStateAsync(
        string schema, string table, Guid aggregateId, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(RiskE2EConfig.ProjectionsConnectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(
            $"SELECT state FROM {schema}.{table} WHERE aggregate_id = @id LIMIT 1", conn);
        cmd.Parameters.AddWithValue("id", aggregateId);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? reader.GetString(0) : null;
    }
}
