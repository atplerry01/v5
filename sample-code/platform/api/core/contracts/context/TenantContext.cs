namespace Whycespace.Platform.Api.Core.Contracts.Context;

/// <summary>
/// Immutable tenant isolation context for multi-tenant execution.
/// Resolved per-request by TenantRegionMiddleware.
/// Platform does NOT mutate this — read-only resolution from identity/request.
/// No default fallback. No ambiguity. Tenant MUST be explicit.
/// </summary>
public sealed record TenantContext
{
    public required Guid TenantId { get; init; }
    public required string TenantType { get; init; }
}
