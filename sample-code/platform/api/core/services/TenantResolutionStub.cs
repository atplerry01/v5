using Whycespace.Platform.Api.Core.Contracts;
using Whycespace.Platform.Api.Core.Contracts.Context;
using Whycespace.Shared.Primitives.Id;

namespace Whycespace.Platform.Api.Core.Services;

/// <summary>
/// Stub implementation of ITenantResolutionService for development/testing.
/// Resolves tenant from identity attributes. Region validation is permissive.
/// Replace with real adapter backed by T0U tenant registry.
/// </summary>
public sealed class TenantResolutionStub : ITenantResolutionService
{
    public Task<TenantContext?> ResolveAsync(WhyceIdentity identity, CancellationToken cancellationToken = default)
    {
        // Derive deterministic tenant from identity
        var tenantId = DeterministicIdHelper.FromSeed($"tenant:{identity.IdentityId}");

        var tenantType = identity.Attributes.TryGetValue("tenantType", out var tt)
            ? tt
            : "SPV";

        return Task.FromResult<TenantContext?>(new TenantContext
        {
            TenantId = tenantId,
            TenantType = tenantType
        });
    }

    public Task<bool> ValidateTenantAccessAsync(
        WhyceIdentity identity, Guid tenantId, CancellationToken cancellationToken = default)
    {
        // Stub: identity can access its own derived tenant only
        var expectedTenantId = DeterministicIdHelper.FromSeed($"tenant:{identity.IdentityId}");
        return Task.FromResult(tenantId == expectedTenantId);
    }

    public Task<bool> ValidateRegionForTenantAsync(
        Guid tenantId, string region, CancellationToken cancellationToken = default)
    {
        // Stub: all regions permitted
        return Task.FromResult(!string.IsNullOrWhiteSpace(region));
    }
}
