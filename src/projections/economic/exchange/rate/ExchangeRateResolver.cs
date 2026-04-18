using System.Text.Json;
using Npgsql;
using Whycespace.Shared.Contracts.Economic.Exchange.Rate;

namespace Whycespace.Projections.Economic.Exchange.Rate;

/// <summary>
/// Phase 4 T4.4 — read-side <see cref="IExchangeRateResolver"/>
/// implementation. Picks the latest Active rate for the (base, quote) pair
/// whose <c>effectiveAt</c> is at or before the supplied transaction time,
/// so the locked rate is deterministic for any given transaction
/// initiation timestamp.
/// </summary>
public sealed class ExchangeRateResolver : IExchangeRateResolver
{
    private const string Sql = @"
        SELECT state
        FROM projection_economic_exchange_rate.exchange_rate_read_model
        WHERE state->>'baseCurrency'  = @base
          AND state->>'quoteCurrency' = @quote
          AND state->>'status'        = 'Active'
          AND (state->>'effectiveAt')::timestamptz <= @asOf
        ORDER BY (state->>'effectiveAt')::timestamptz DESC
        LIMIT 1";

    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

    private readonly NpgsqlDataSource _dataSource;

    public ExchangeRateResolver(NpgsqlDataSource dataSource) => _dataSource = dataSource;

    public async Task<ExchangeRateResolution?> ResolveAsync(
        string baseCurrency,
        string quoteCurrency,
        DateTimeOffset asOf,
        CancellationToken cancellationToken = default)
    {
        await using var conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(Sql, conn);
        cmd.Parameters.AddWithValue("base", baseCurrency);
        cmd.Parameters.AddWithValue("quote", quoteCurrency);
        cmd.Parameters.AddWithValue("asOf", asOf);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return null;

        var json = reader.GetString(0);
        var model = JsonSerializer.Deserialize<ExchangeRateReadModel>(json, Json);
        if (model is null) return null;

        return new ExchangeRateResolution(
            model.RateId,
            model.BaseCurrency,
            model.QuoteCurrency,
            model.RateValue,
            model.EffectiveAt,
            model.VersionNumber);
    }
}
