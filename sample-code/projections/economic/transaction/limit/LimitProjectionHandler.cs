using System.Text.Json;
using Whycespace.Shared.Contracts.Projection;
using Whycespace.Shared.Contracts.Infrastructure;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Projections.Economic;

/// <summary>
/// Projects daily and monthly limit usage per identity.
/// Time buckets derived from event.Timestamp (source of truth):
///   DailyBucket  = YYYY-MM-DD
///   MonthlyBucket = YYYY-MM
/// Global ordering: Timestamp + Version tiebreaker. Older events skipped.
/// </summary>
public sealed class LimitUsageProjectionHandler : IdempotentEconomicProjectionHandler
{
    private readonly EconomicReadStore _readStore;

    public LimitUsageProjectionHandler(EconomicReadStore readStore, IClock clock) : base(clock)
    {
        _readStore = readStore ?? throw new ArgumentNullException(nameof(readStore));
    }

    public override string ProjectionName => "economic.limit-usage";

    public override string[] EventTypes =>
    [
        "economic.transaction.initiated",
        "economic.transaction.approved",
        "economic.limit.exceeded"
    ];

    protected override async Task ApplyAsync(
        ProjectionEvent @event,
        IProjectionStore store,
        CancellationToken cancellationToken)
    {
        var json = ParsePayload(@event);
        if (json is null) return;

        var identityId = GetString(json.Value, "IdentityId")
            ?? GetHeader(@event, "x-identity-id");
        if (identityId is null) return;

        var existing = await _readStore.GetLimitUsageAsync(identityId, cancellationToken);

        // Global ordering guard
        if (existing is not null &&
            ShouldSkipEvent(@event.Timestamp, @event.Version,
                existing.LastEventTimestamp, existing.LastEventVersion))
            return;

        var dailyBucket = @event.Timestamp.ToString("yyyy-MM-dd");
        var monthlyBucket = @event.Timestamp.ToString("yyyy-MM");

        // Reset counters on bucket boundary change
        var dailyUsage = existing?.DailyBucket == dailyBucket ? existing.DailyUsage : 0m;
        var dailyCount = existing?.DailyBucket == dailyBucket ? existing.DailyTransactionCount : 0;
        var monthlyUsage = existing?.MonthlyBucket == monthlyBucket ? existing.MonthlyUsage : 0m;
        var monthlyCount = existing?.MonthlyBucket == monthlyBucket ? existing.MonthlyTransactionCount : 0;
        var limitExceededCount = existing?.LimitExceededCount ?? 0;

        if (@event.EventType == "economic.limit.exceeded")
        {
            limitExceededCount++;
        }
        else
        {
            var amount = GetDecimal(json.Value, "Amount");
            dailyUsage += amount;
            monthlyUsage += amount;
            dailyCount++;
            monthlyCount++;
        }

        var updated = new LimitUsageReadModel
        {
            IdentityId = identityId,
            DailyUsage = dailyUsage,
            MonthlyUsage = monthlyUsage,
            DailyTransactionCount = dailyCount,
            MonthlyTransactionCount = monthlyCount,
            DailyBucket = dailyBucket,
            MonthlyBucket = monthlyBucket,
            LimitExceededCount = limitExceededCount,
            LastUpdated = @event.Timestamp,
            LastEventTimestamp = @event.Timestamp,
            LastEventVersion = @event.Version
        };

        await _readStore.SetLimitUsageAsync(identityId, updated, cancellationToken);
    }

    private static JsonElement? ParsePayload(ProjectionEvent @event)
    {
        if (@event.Payload is JsonElement je) return je;
        if (@event.Payload is null) return null;
        var s = JsonSerializer.Serialize(@event.Payload);
        return JsonDocument.Parse(s).RootElement;
    }

    private static string? GetString(JsonElement json, string prop)
        => json.TryGetProperty(prop, out var v) ? v.GetString() : null;

    private static decimal GetDecimal(JsonElement json, string prop)
        => json.TryGetProperty(prop, out var v) && v.TryGetDecimal(out var d) ? d : 0m;

    private static string? GetHeader(ProjectionEvent @event, string key)
        => @event.Headers.TryGetValue(key, out var v) ? v : null;
}
