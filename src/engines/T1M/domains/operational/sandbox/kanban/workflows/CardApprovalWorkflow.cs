namespace Whyce.Engines.T1M.Domains.Operational.Sandbox.Kanban.Workflows;

/// <summary>
/// Canonical workflow-name constant for the Kanban card approval lifecycle.
/// Registered in KanbanBootstrap.RegisterWorkflows.
/// </summary>
public static class CardApprovalWorkflowNames
{
    public const string Approve = "kanban.card.approval";
}

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
