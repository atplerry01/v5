using Npgsql;

namespace Whycespace.Tests.E2E.Economic.Revenue.Setup;

/// <summary>
/// Phase 3 (T3.7) projection-side assertions. Polls the canonical read models
/// to verify the deterministic pipeline propagated end-to-end:
///   distribution_read_model.status == "Confirmed" (and later "Paid"),
///   payout_read_model.status == "Executed",
///   journal_read_model row exists for the derived JournalId.
/// All polls share a single timeout so the suite fails fast on a stuck stage.
/// </summary>
public static class PipelineProjectionVerifier
{
    public static async Task<string?> WaitForDistributionStatusAsync(
        Guid distributionId, string expectedStatus, TimeSpan timeout, CancellationToken ct = default) =>
            await PollScalarAsync(
                "SELECT status FROM projection_economic_revenue_distribution.distribution_read_model WHERE distribution_id = @id",
                "@id", distributionId,
                expected: s => string.Equals(s, expectedStatus, StringComparison.Ordinal),
                timeout, ct);

    public static async Task<string?> WaitForPayoutStatusAsync(
        Guid payoutId, string expectedStatus, TimeSpan timeout, CancellationToken ct = default) =>
            await PollScalarAsync(
                "SELECT status FROM projection_economic_revenue_payout.payout_read_model WHERE payout_id = @id",
                "@id", payoutId,
                expected: s => string.Equals(s, expectedStatus, StringComparison.Ordinal),
                timeout, ct);

    public static async Task<bool> WaitForJournalAsync(
        Guid journalId, TimeSpan timeout, CancellationToken ct = default)
    {
        var status = await PollScalarAsync(
            "SELECT 'present' FROM projection_economic_ledger_journal.journal_read_model WHERE journal_id = @id",
            "@id", journalId,
            expected: _ => true,
            timeout, ct);
        return status == "present";
    }

    private static async Task<string?> PollScalarAsync(
        string sql, string paramName, Guid id, Func<string, bool> expected, TimeSpan timeout, CancellationToken ct)
    {
        var deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline)
        {
            await using var conn = new NpgsqlConnection(RevenuePipelineE2EConfig.ProjectionsConnectionString);
            await conn.OpenAsync(ct);
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue(paramName, id);
            var raw = await cmd.ExecuteScalarAsync(ct);
            var value = raw?.ToString();
            if (value is not null && expected(value)) return value;
            await Task.Delay(250, ct);
        }
        return null;
    }
}
