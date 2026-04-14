namespace Whycespace.Shared.Contracts.Operational.Sandbox.Kanban.Workflow;

/// <summary>
/// Workflow payload for the card approval lifecycle.
/// Carries the coordinates needed by each step to dispatch T2E commands.
/// </summary>
public sealed record CardApprovalIntent(
    Guid BoardId,
    Guid CardId,
    Guid FromListId,
    Guid ReviewListId,
    string UserId);
