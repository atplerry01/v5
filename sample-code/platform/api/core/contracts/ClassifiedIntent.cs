using Whycespace.Platform.Api.Core.Contracts.Context;

namespace Whycespace.Platform.Api.Core.Contracts;

/// <summary>
/// Output of intent classification. Maps a user-facing IntentType to
/// system routing coordinates (classification, domain, workflow key).
/// Platform does NOT interpret these — it forwards to downstream/WSS.
///
/// Metadata carries structural context (identityId, correlationId, traceId, timestamp)
/// for traceability. No business data in metadata.
/// Tenant and Region are propagated for multi-tenant/multi-region isolation.
/// </summary>
public sealed record ClassifiedIntent
{
    public required string Classification { get; init; }
    public required string Domain { get; init; }
    public required string WorkflowKey { get; init; }
    public required string Cluster { get; init; }
    public required string Subcluster { get; init; }
    public string? Context { get; init; }
    public TenantContext? Tenant { get; init; }
    public RegionContext? Region { get; init; }
    public IReadOnlyDictionary<string, string>? Metadata { get; init; }
}
