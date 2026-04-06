using System.Text.Json;
using Whycespace.Projections.IdentityFederation.ReadModels;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.IdentityFederation.Handlers;

/// <summary>
/// Projection handler for issuer.registered events. Idempotent and replay-safe.
/// </summary>
public static class IssuerRegisteredHandler
{
    public static async Task HandleAsync(
        ProjectionEvent @event,
        Queries.FederationReadStore store,
        CancellationToken ct)
    {
        if (@event.Payload is null) return;

        var json = JsonSerializer.Serialize(@event.Payload);
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var issuerId = root.GetProperty("IssuerId").GetGuid().ToString();

        var existing = await store.GetIssuerAsync(issuerId, ct);
        if (existing is not null) return; // idempotent

        var model = new IssuerReadModel
        {
            IssuerId = issuerId,
            Name = root.GetProperty("Name").GetString()!,
            IssuerType = root.GetProperty("IssuerType").GetString()!,
            TrustLevel = 0m,
            Status = "Pending",
            CreatedAt = @event.Timestamp
        };

        await store.SetIssuerAsync(issuerId, model, ct);
    }
}
