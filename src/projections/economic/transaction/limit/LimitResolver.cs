using System.Text.Json;
using Npgsql;
using Whycespace.Shared.Contracts.Economic.Transaction.Limit;

namespace Whycespace.Projections.Economic.Transaction.Limit;

/// <summary>
/// Phase 4 T4.1 — read-side <see cref="ILimitResolver"/> implementation.
/// Resolves the active limit for a given (accountId, currency) pair from
/// <c>projection_economic_transaction_limit.limit_read_model</c>. Returns
/// <c>null</c> when no Active limit exists for the account so the
/// CheckLimitStep can no-op for un-bounded accounts.
///
/// Index hint: this query filters on JSONB fields (`accountId`, `currency`,
/// `status`). For high-throughput control planes, add a Postgres GIN
/// index on `state` or expression indexes on the three keys; absent any
/// index the query is a full scan over the limit projection.
/// </summary>
public sealed class LimitResolver : ILimitResolver
{
    private const string Sql = @"
        SELECT state
        FROM projection_economic_transaction_limit.limit_read_model
        WHERE (state->>'accountId')::uuid = @accountId
          AND state->>'currency' = @currency
          AND state->>'status' = 'Active'
        ORDER BY (state->>'definedAt') DESC
        LIMIT 1";

    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

    private readonly NpgsqlDataSource _dataSource;

    public LimitResolver(NpgsqlDataSource dataSource) => _dataSource = dataSource;

    public async Task<LimitResolution?> ResolveAsync(
        Guid accountId,
        string currency,
        CancellationToken cancellationToken = default)
    {
        await using var conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(Sql, conn);
        cmd.Parameters.AddWithValue("accountId", accountId);
        cmd.Parameters.AddWithValue("currency", currency);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return null;

        var json = reader.GetString(0);
        var model = JsonSerializer.Deserialize<LimitReadModel>(json, Json);
        if (model is null) return null;

        return new LimitResolution(
            model.LimitId,
            model.AccountId,
            model.Currency,
            model.Threshold,
            model.CurrentUtilization,
            model.Status);
    }
}
