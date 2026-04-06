using System.Text.Json;
using Whycespace.Engines.T0U.WhyceChain.Write;
using Whycespace.Shared.Contracts.Chain;
using Whycespace.Shared.Contracts.Infrastructure.Storage;
using Whycespace.Shared.Utilities;

namespace Whycespace.Engines.T0U.WhyceChain.Identity;

/// <summary>
/// Anchors identity-critical domain events to the WhyceChain.
/// Only events in the anchor set are written to the immutable ledger.
///
/// Anchored events:
///   identity.created, identity.verified,
///   credential.issued, credential.revoked,
///   access.granted, access.revoked,
///   consent.given, consent.withdrawn,
///   trust.updated
/// </summary>
public sealed class IdentityChainAnchor : ChainEngineBase, IIdentityChainAnchor
{
    private readonly IChainWriter _writer;

    private static readonly HashSet<string> AnchoredEventTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "identity.created",
        "identity.verified",
        "credential.issued",
        "credential.revoked",
        "access.granted",
        "access.revoked",
        "consent.given",
        "consent.withdrawn",
        "trust.updated",

        // Domain event class names (alternate form)
        "IdentityCreatedEvent",
        "IdentityVerifiedEvent",
        "CredentialIssuedEvent",
        "CredentialRevokedEvent",
        "AccessGrantedEvent",
        "AccessRevokedEvent",
        "ConsentGrantedEvent",
        "ConsentGrantedEvent",
        "ConsentRevokedEvent",
        "ConsentWithdrawnEvent",
        "TrustUpdatedEvent",
        "TrustScoreUpdatedEvent",

        // Federation events (E10 — chain-anchored)
        "issuer.registered",
        "issuer.approved",
        "issuer.suspended",
        "issuer.revoked",
        "identity.linked",
        "identity.unlinked",
        "trust.evaluated",
        "trust.degraded",

        // Federation domain event class names
        "IssuerRegisteredEvent",
        "IssuerApprovedEvent",
        "IssuerSuspendedEvent",
        "IssuerRevokedEvent",
        "IdentityLinkedEvent",
        "IdentityUnlinkedEvent",
        "TrustEvaluatedEvent",
        "TrustDegradedEvent"
    };

    public IdentityChainAnchor(IChainWriter writer)
    {
        _writer = writer ?? throw new ArgumentNullException(nameof(writer));
    }

    /// <summary>
    /// Returns true if this event type MUST be anchored to the chain.
    /// </summary>
    public static bool MustAnchorStatic(string eventType)
    {
        return AnchoredEventTypes.Contains(eventType);
    }

    /// <inheritdoc />
    bool IIdentityChainAnchor.MustAnchor(string eventType)
    {
        return AnchoredEventTypes.Contains(eventType);
    }

    /// <summary>
    /// Anchor an identity-critical event to the WhyceChain.
    /// Computes payload hash from event data and writes an immutable block.
    ///
    /// FAILURE to anchor → command MUST fail. No async inconsistency.
    /// </summary>
    public async Task<ChainWriteResult> AnchorAsync(
        AnchorRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!MustAnchorStatic(request.EventType))
            throw new InvalidOperationException(
                $"Event type '{request.EventType}' is not in the identity anchor set.");

        var eventDataHash = HashUtility.ComputeSha256(
            JsonSerializer.Serialize(request.EventData, ChainSerializerOptions.Default));

        var payload = new ChainPayload
        {
            EventId = request.EventId,
            AggregateId = request.AggregateId,
            EventType = request.EventType,
            EventDataHash = eventDataHash,
            PolicyDecisionHash = request.PolicyDecisionHash,
            ExecutionHash = request.ExecutionHash,
            OccurredAt = request.OccurredAt
        };

        return await _writer.WriteAsync(payload, cancellationToken);
    }
}

