using System.Text.Json;
using Whycespace.Shared.Contracts.Projection;
using Whycespace.Shared.Contracts.Infrastructure;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Projections.Economic;

/// <summary>
/// Projects active enforcement state per identity.
/// Sourced from EnforcementAppliedEvent and EnforcementReleasedEvent.
/// Global ordering: Timestamp + Version tiebreaker. Older events skipped.
/// </summary>
public sealed class EnforcementProjectionHandler : IdempotentEconomicProjectionHandler
{
    private readonly EconomicReadStore _readStore;

    public EnforcementProjectionHandler(EconomicReadStore readStore, IClock clock) : base(clock)
    {
        _readStore = readStore ?? throw new ArgumentNullException(nameof(readStore));
    }

    public override string ProjectionName => "economic.enforcement";

    public override string[] EventTypes =>
    [
        "economic.enforcement.applied",
        "economic.enforcement.released"
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

        var existing = await _readStore.GetEnforcementAsync(identityId, cancellationToken);

        // Global ordering guard
        if (existing is not null &&
            ShouldSkipEvent(@event.Timestamp, @event.Version,
                existing.LastEventTimestamp, existing.LastEventVersion))
            return;

        if (@event.EventType == "economic.enforcement.applied")
        {
            var model = new EnforcementReadModel
            {
                IdentityId = identityId,
                EnforcementType = GetString(json.Value, "EnforcementType") ?? "unknown",
                Reason = GetString(json.Value, "Reason") ?? "",
                IsActive = true,
                AppliedAt = @event.Timestamp,
                LastUpdated = @event.Timestamp,
                LastEventTimestamp = @event.Timestamp,
                LastEventVersion = @event.Version
            };

            await _readStore.SetEnforcementAsync(identityId, model, cancellationToken);
        }
        else if (@event.EventType == "economic.enforcement.released")
        {
            var released = (existing ?? new EnforcementReadModel
            {
                IdentityId = identityId,
                EnforcementType = "unknown",
                Reason = "",
                IsActive = false,
                AppliedAt = default
            }) with
            {
                IsActive = false,
                ReleasedAt = @event.Timestamp,
                LastUpdated = @event.Timestamp,
                LastEventTimestamp = @event.Timestamp,
                LastEventVersion = @event.Version
            };

            await _readStore.SetEnforcementAsync(identityId, released, cancellationToken);
        }
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

    private static string? GetHeader(ProjectionEvent @event, string key)
        => @event.Headers.TryGetValue(key, out var v) ? v : null;
}
