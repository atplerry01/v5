namespace Whycespace.Projections.Workflow;

/// <summary>
/// Maps workflow step executions ↔ policy decision hashes for audit.
/// Projected from anchored events that carry workflow context.
/// Key = "workflow-policy:{decisionHash}".
/// </summary>
public sealed record WorkflowPolicyReadModel
{
    public required string DecisionHash { get; init; }
    public required string WorkflowId { get; init; }
    public required string StepId { get; init; }
    public required string State { get; init; }
    public required string Transition { get; init; }
    public required string PolicyId { get; init; }
    public required string Decision { get; init; }
    public required string SubjectId { get; init; }
    public string? BlockId { get; init; }
    public string? BlockHash { get; init; }
    public required DateTimeOffset AnchoredAt { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
    public long LastEventVersion { get; init; }

    public static string KeyFor(string decisionHash) => $"workflow-policy:{decisionHash}";
    public static string KeyByWorkflow(string workflowId) => $"workflow-policy-by-workflow:{workflowId}";
    public static string KeyByStep(string stepId) => $"workflow-policy-by-step:{stepId}";
}

/// <summary>
/// Index of workflow-policy links by workflowId.
/// </summary>
public sealed record WorkflowPolicyIndexReadModel
{
    public required string IndexKey { get; init; }
    public List<string> DecisionHashes { get; init; } = [];
    public int Count { get; init; }
    public DateTimeOffset LastUpdated { get; init; }

    public static string KeyByWorkflow(string workflowId) => $"workflow-policy-by-workflow:{workflowId}";
    public static string KeyByStep(string stepId) => $"workflow-policy-by-step:{stepId}";
}
