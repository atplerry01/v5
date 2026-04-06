using System.Text.Json;

namespace Whycespace.Platform.Api.Core.Contracts.Context;

/// <summary>
/// Well-known header keys for carrying TenantContext and RegionContext through the platform pipeline.
/// TenantRegionMiddleware writes these; downstream middleware and controller read them.
/// Mirrors the IdentityHeaderKeys pattern for tenant/region isolation.
/// </summary>
public static class TenantRegionHeaderKeys
{
    public const string TenantId = "X-Whyce-TenantId";
    public const string TenantType = "X-Whyce-TenantType";
    public const string Region = "X-Whyce-Region";
    public const string Jurisdiction = "X-Whyce-Jurisdiction";

    /// <summary>
    /// Writes TenantContext and RegionContext fields into the headers dictionary.
    /// </summary>
    public static IReadOnlyDictionary<string, string> Enrich(
        IReadOnlyDictionary<string, string> existing,
        TenantContext tenant,
        RegionContext region)
    {
        var headers = new Dictionary<string, string>(existing)
        {
            [TenantId] = tenant.TenantId.ToString(),
            [TenantType] = tenant.TenantType,
            [Region] = region.Region,
            [Jurisdiction] = region.Jurisdiction
        };

        return headers;
    }

    /// <summary>
    /// Extracts TenantContext from headers written by TenantRegionMiddleware.
    /// Returns null if tenant headers are not present.
    /// </summary>
    public static TenantContext? ExtractTenant(IReadOnlyDictionary<string, string> headers)
    {
        if (!headers.TryGetValue(TenantId, out var idStr) || !Guid.TryParse(idStr, out var tenantId))
            return null;

        if (!headers.TryGetValue(TenantType, out var tenantType) || string.IsNullOrWhiteSpace(tenantType))
            return null;

        return new TenantContext
        {
            TenantId = tenantId,
            TenantType = tenantType
        };
    }

    /// <summary>
    /// Extracts RegionContext from headers written by TenantRegionMiddleware.
    /// Returns null if region headers are not present.
    /// </summary>
    public static RegionContext? ExtractRegion(IReadOnlyDictionary<string, string> headers)
    {
        if (!headers.TryGetValue(Region, out var region) || string.IsNullOrWhiteSpace(region))
            return null;

        if (!headers.TryGetValue(Jurisdiction, out var jurisdiction) || string.IsNullOrWhiteSpace(jurisdiction))
            return null;

        return new RegionContext
        {
            Region = region,
            Jurisdiction = jurisdiction
        };
    }
}
