namespace Whycespace.Systems.Midstream.Wss.Workflows.Governance.Policy;

/// <summary>
/// Declarative workflow definitions for policy governance lifecycle.
/// STRICTLY DECLARATIVE — no conditionals, no loops, no business logic.
/// Each workflow maps to a single engine invocation via Runtime.
/// </summary>
public static class PolicyWorkflowDefinition
{
    public static readonly string WorkflowId = "governance.policy";
    public static readonly string WorkflowPrefix = "governance.policy";
    public static readonly string WorkflowVersion = "v1";

    public static readonly IReadOnlyList<string> Actions =
    [
        "proposal.submit",
        "approve",
        "activate"
    ];

    /// <summary>
    /// Resolves the fully-qualified versioned workflow identifier.
    /// Format: {workflowId}:{version}
    /// </summary>
    public static string ResolveVersionedId(string action)
    {
        return $"{WorkflowPrefix}.{action}:{WorkflowVersion}";
    }
}
