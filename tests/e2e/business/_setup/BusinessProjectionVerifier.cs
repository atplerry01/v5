using Npgsql;

namespace Whycespace.Tests.E2E.Business.Setup;

/// <summary>
/// Direct projection-schema verification for business E2E tests. Polls the
/// projection table for an aggregate row (until a deadline, since projection
/// updates are async via Kafka), and asserts presence/absence per spec.
///
/// Read-only — never mutates. Uses the same JSONB layout written by
/// PostgresProjectionStore: SELECT state FROM {schema}.{table} WHERE aggregate_id = @id.
///
/// Mirror of <see cref="Whycespace.Tests.E2E.Economic.Capital.Setup.ProjectionVerifier"/>
/// but bound to the business-system connection string.
/// </summary>
public static class BusinessProjectionVerifier
{
    /// <summary>
    /// Polls the projection table for the aggregate row. Returns the JSONB
    /// state string when the row appears; throws TimeoutException if it does
    /// not appear before <paramref name="timeout"/>.
    /// </summary>
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

    /// <summary>
    /// Asserts no projection row exists for the aggregate id. Used by failure-case
    /// tests to verify "no projection update" after a rejected command.
    /// </summary>
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
        await using var conn = new NpgsqlConnection(BusinessE2EConfig.ProjectionsConnectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(
            $"SELECT state FROM {schema}.{table} WHERE aggregate_id = @id LIMIT 1", conn);
        cmd.Parameters.AddWithValue("id", aggregateId);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? reader.GetString(0) : null;
    }
}
