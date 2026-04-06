using System.Text.Json;
using Whycespace.Projections.IdentityFederation.ReadModels;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.IdentityFederation.Handlers;

/// <summary>
/// Projection handler for identity.linked events. Idempotent and replay-safe.
/// </summary>
public static class IdentityLinkedHandler
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

        var identityId = root.GetProperty("IdentityId").GetGuid().ToString();
        var externalId = root.GetProperty("ExternalId").GetString()!;
        var issuerId = root.GetProperty("IssuerId").GetGuid().ToString();

        var linkKey = $"{externalId}:{issuerId}";

        var model = new FederationLinkReadModel
        {
            IdentityId = identityId,
            ExternalId = externalId,
            IssuerId = issuerId,
            Confidence = root.GetProperty("InitialConfidence").GetDecimal(),
            VerificationLevel = root.GetProperty("VerificationLevel").GetInt32(),
            Status = "Active",
            ProvenanceSource = root.GetProperty("ProvenanceSource").GetString()!,
            LinkedAt = @event.Timestamp
        };

        await store.SetLinkAsync(identityId, linkKey, model, ct);
    }
}
