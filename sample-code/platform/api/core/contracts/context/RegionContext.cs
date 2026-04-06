namespace Whycespace.Platform.Api.Core.Contracts.Context;

/// <summary>
/// Immutable region/jurisdiction context for region-aware execution.
/// Resolved per-request by TenantRegionMiddleware.
/// Platform does NOT mutate this — read-only resolution from request/headers.
/// No default fallback. No ambiguity. Region MUST be explicit.
/// </summary>
public sealed record RegionContext
{
    public required string Region { get; init; }
    public required string Jurisdiction { get; init; }
}
