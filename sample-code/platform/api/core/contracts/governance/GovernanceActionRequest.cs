namespace Whycespace.Platform.Api.Core.Contracts.Governance;

/// <summary>
/// Request to submit a governance action via the WhycePlus platform.
/// Actions are routed through WSS workflows — platform NEVER decides approval.
///
/// Valid actions: PROPOSE, APPROVE, REJECT
/// All actions pass through WhycePolicy enforcement at runtime.
/// </summary>
public sealed record GovernanceActionRequest
{
    public required Guid DecisionId { get; init; }
    public required string Action { get; init; }
    public required object Payload { get; init; }
    public string? Justification { get; init; }
}
