namespace Whyce.Engines.T1M.Domains.Operational.Sandbox.Kanban.State;

public enum CardApprovalStatus
{
    Pending,
    InReview,
    Approved,
    Completed
}

/// <summary>
/// Pure typed workflow state for the Card Approval workflow.
/// Serialized into WorkflowExecutionContext via GetState/SetState.
/// No dictionary keys, no string parsing — the serializer handles round-trips.
/// </summary>
public sealed class CardApprovalState
{
    public Guid BoardId { get; init; }
    public Guid CardId { get; init; }
    public Guid FromListId { get; init; }
    public Guid ReviewListId { get; init; }
    public string CurrentStep { get; set; } = string.Empty;
    public CardApprovalStatus Status { get; set; } = CardApprovalStatus.Pending;

    /// <summary>
    /// Creates a fully initialized state from the workflow intent. Called once by ValidateCardStep.
    /// </summary>
    public static CardApprovalState Create(
        Guid boardId,
        Guid cardId,
        Guid fromListId,
        Guid reviewListId) => new()
    {
        BoardId = boardId,
        CardId = cardId,
        FromListId = fromListId,
        ReviewListId = reviewListId,
        CurrentStep = CardApprovalSteps.Validate,
        Status = CardApprovalStatus.Pending
    };
}
