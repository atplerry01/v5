using Whycespace.Platform.Api.Core.Contracts.Governance;

namespace Whycespace.Platform.Api.Core.Services.Governance;

/// <summary>
/// Governance action submission service.
/// Routes governance actions (PROPOSE / APPROVE / REJECT) through WSS workflows.
///
/// Platform is NEVER the decision authority.
/// Platform only forwards the request — WhycePolicy decides at runtime.
///
/// Flow: Platform → DownstreamAdapter → ProcessHandler → WSS → Runtime → T0U Governance Engine
///
/// MUST NOT:
/// - Execute governance logic
/// - Call policy engine directly
/// - Approve or reject decisions
/// - Call engines or domain services
/// </summary>
public interface IGovernanceActionService
{
    Task<GovernanceActionResult> SubmitActionAsync(
        GovernanceActionRequest request,
        string whyceId,
        string correlationId,
        string traceId,
        CancellationToken cancellationToken = default);
}
