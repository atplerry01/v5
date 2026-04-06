namespace Whycespace.Platform.Api.Core.Contracts.Governance;

/// <summary>
/// Result of a governance action submission.
/// Contains the workflow ID for tracking the action through WSS.
/// Platform does NOT report approval/rejection — that comes from the governance engine.
/// </summary>
public sealed record GovernanceActionResult
{
    public required Guid WorkflowId { get; init; }
    public required string Status { get; init; }
    public string? ErrorMessage { get; init; }

    public bool IsAccepted => string.Equals(Status, "ACCEPTED", StringComparison.OrdinalIgnoreCase);

    public static GovernanceActionResult Accepted(Guid workflowId) => new()
    {
        WorkflowId = workflowId,
        Status = "ACCEPTED"
    };

    public static GovernanceActionResult Failed(string error) => new()
    {
        WorkflowId = Guid.Empty,
        Status = "FAILED",
        ErrorMessage = error
    };
}
