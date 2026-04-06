using Whycespace.Platform.Api.Core.Contracts;
using Whycespace.Platform.Api.Core.Contracts.Context;

namespace Whycespace.Platform.Api.Core.Services;

/// <summary>
/// WhycePolicy preview adapter for the platform layer.
/// Evaluates policy BEFORE execution — advisory only.
/// Delegates to WhycePolicy (T0U) via runtime control plane.
///
/// Provides explainable decisions with reasons, violations, and policy IDs.
/// Policy input includes tenantId, region, and jurisdiction for jurisdiction-specific
/// and tenant-specific policy rules.
/// MUST NOT block execution — runtime WhycePolicy is the final authority.
/// MUST NOT evaluate policy logic — pure delegation.
/// MUST NOT mutate state.
/// </summary>
public interface IPolicyPreviewService
{
    /// <summary>
    /// Previews a policy evaluation for the given intent, route, identity, and tenant/region context.
    /// ClassifiedIntent and IntentRoute now carry Tenant + Region fields.
    /// Returns an advisory PolicyPreview with decision, reasons, and violations.
    /// If the preview service fails, returns PolicyPreview.ServiceUnavailable() — never throws.
    /// </summary>
    Task<PolicyPreview> PreviewAsync(
        ClassifiedIntent intent,
        IntentRoute route,
        WhyceIdentity? identity,
        string correlationId,
        CancellationToken cancellationToken = default);
}
