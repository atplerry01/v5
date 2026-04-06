using Whycespace.Platform.Api.Core.Contracts;
using Whycespace.Platform.Api.Core.Contracts.Context;

namespace Whycespace.Platform.Api.Core.Services;

/// <summary>
/// Tenant resolution adapter for the platform layer.
/// Resolves TenantContext from identity profile (read-only).
/// Platform never stores or mutates tenant state — pure resolution.
///
/// MUST NOT embed business rules.
/// MUST NOT provide default tenant fallback.
/// MUST validate identity belongs to resolved tenant.
/// </summary>
public interface ITenantResolutionService
{
    /// <summary>
    /// Resolves tenant context from the authenticated identity.
    /// Returns null if identity has no tenant association.
    /// </summary>
    Task<TenantContext?> ResolveAsync(WhyceIdentity identity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that the identity is authorized to access the given tenant.
    /// Returns false if cross-tenant access is not explicitly permitted.
    /// </summary>
    Task<bool> ValidateTenantAccessAsync(
        WhyceIdentity identity,
        Guid tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that the given region is allowed for the tenant.
    /// Returns false if the tenant is not permitted to operate in the region.
    /// </summary>
    Task<bool> ValidateRegionForTenantAsync(
        Guid tenantId,
        string region,
        CancellationToken cancellationToken = default);
}
