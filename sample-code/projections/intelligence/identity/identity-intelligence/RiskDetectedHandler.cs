using System.Text.Json;
using Whycespace.Projections.IdentityIntelligence.ReadModels;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.IdentityIntelligence.Handlers;

/// <summary>
/// Projection handler for risk.detected events.
/// Idempotent and replay-safe.
/// </summary>
public static class RiskDetectedHandler
{
    public static async Task HandleAsync(
        ProjectionEvent @event,
        Queries.IntelligenceReadStore store,
        CancellationToken ct)
    {
        if (@event.Payload is null) return;

        var json = JsonSerializer.Serialize(@event.Payload);
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var identityId = root.GetProperty("IdentityId").GetString()!;
        var score = root.GetProperty("Score").GetDecimal();
        var severity = root.GetProperty("Severity").GetString()!;

        var flags = new List<AnomalyFlagReadModel>();
        if (root.TryGetProperty("Flags", out var flagsElement))
        {
            foreach (var flag in flagsElement.EnumerateArray())
            {
                flags.Add(new AnomalyFlagReadModel
                {
                    AnomalyType = flag.GetString() ?? "",
                    Description = "",
                    Confidence = 0m,
                    DetectedAt = @event.Timestamp
                });
            }
        }

        var model = new RiskScoreReadModel
        {
            IdentityId = identityId,
            Score = score,
            Severity = severity,
            Flags = flags,
            ComputedAt = @event.Timestamp
        };

        await store.SetRiskAsync(identityId, model, ct);
    }
}
