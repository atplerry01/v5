using Whycespace.Platform.Api.Core.Contracts.Context;

namespace Whycespace.Platform.Api.Core.Contracts;

/// <summary>
/// Mandatory request envelope for all WhycePlus platform entry.
/// Immutable. Schema-validated at the platform boundary.
/// No business logic — pure data transport.
/// </summary>
public sealed record WhyceRequest
{
    public required string IdentityId { get; init; }
    public required string Intent { get; init; }
    public required string IntentType { get; init; }
    public required object Payload { get; init; }
    public required string Jurisdiction { get; init; }
    public TenantContext? Tenant { get; init; }
    public RegionContext? Region { get; init; }
    public IReadOnlyDictionary<string, string>? IntentData { get; init; }
}
