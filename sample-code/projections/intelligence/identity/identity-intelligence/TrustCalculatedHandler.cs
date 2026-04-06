using System.Text.Json;
using Whycespace.Projections.IdentityIntelligence.ReadModels;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.IdentityIntelligence.Handlers;

/// <summary>
/// Projection handler for trust.calculated events.
/// Idempotent and replay-safe.
/// </summary>
public static class TrustCalculatedHandler
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
        var classification = root.GetProperty("Classification").GetString()!;

        var model = new TrustScoreReadModel
        {
            IdentityId = identityId,
            Score = score,
            Classification = classification,
            ComputedAt = @event.Timestamp
        };

        await store.SetTrustAsync(identityId, model, ct);
    }
}
