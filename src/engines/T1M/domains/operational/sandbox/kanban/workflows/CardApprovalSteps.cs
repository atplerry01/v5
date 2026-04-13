namespace Whycespace.Engines.T1M.Domains.Operational.Sandbox.Kanban.State;

/// <summary>
/// Canonical step identity constants for the Card Approval workflow.
/// Used by steps when setting CurrentStep and by the workflow definition
/// for deterministic step identification.
/// </summary>
public static class CardApprovalSteps
{
    public const string Validate = "validate";
    public const string MoveToReview = "move_to_review";
    public const string Approve = "approve";
    public const string Complete = "complete";
}
