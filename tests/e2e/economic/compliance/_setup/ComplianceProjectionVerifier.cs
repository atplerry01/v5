using Npgsql;

namespace Whycespace.Tests.E2E.Economic.Compliance.Setup;

/// <summary>
/// Direct projection-schema verification for compliance E2E tests. Polls the
/// projection table for an aggregate row (until a deadline, since projection
/// updates are async via Kafka), and asserts presence/absence per spec.
/// Mirrors the capital/exchange verifiers.
///
/// Read-only — never mutates. Uses the same JSONB layout written by
/// PostgresProjectionStore: SELECT state FROM {schema}.{table} WHERE aggregate_id = @id.
/// </summary>
public static class ComplianceProjectionVerifier
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
    /// Polls the projection table until the JSONB <c>state.status</c> (or
    /// <c>Status</c>) field equals <paramref name="expectedStatus"/>. Used to
    /// wait for the Draft → Finalized transition after FinalizeAuditRecord.
    /// </summary>
    public static async Task<string> PollUntilStatusAsync(
        string schema, string table, Guid aggregateId, string expectedStatus, TimeSpan timeout, CancellationToken ct = default)
    {
        var deadline = DateTime.UtcNow + timeout;
        string? lastObserved = null;
        while (DateTime.UtcNow < deadline)
        {
            var json = await TryLoadStateAsync(schema, table, aggregateId, ct);
            if (json is not null)
            {
                lastObserved = json;
                if (JsonFieldMatches(json, "Status", expectedStatus) ||
                    JsonFieldMatches(json, "status", expectedStatus))
                    return json;
            }
            await Task.Delay(200, ct);
        }
        throw new TimeoutException(
            $"Projection row did not reach status='{expectedStatus}' within {timeout.TotalSeconds:F0}s for {schema}.{table} aggregate_id={aggregateId}. " +
            $"Last observed state: {lastObserved ?? "<absent>"}.");
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

    private static bool JsonFieldMatches(string json, string field, string expected)
        => json.Contains($"\"{field}\":\"{expected}\"", StringComparison.Ordinal)
           || json.Contains($"\"{field}\": \"{expected}\"", StringComparison.Ordinal);

    private static async Task<string?> TryLoadStateAsync(
        string schema, string table, Guid aggregateId, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(ComplianceE2EConfig.ProjectionsConnectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(
            $"SELECT state FROM {schema}.{table} WHERE aggregate_id = @id LIMIT 1", conn);
        cmd.Parameters.AddWithValue("id", aggregateId);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? reader.GetString(0) : null;
    }
}
