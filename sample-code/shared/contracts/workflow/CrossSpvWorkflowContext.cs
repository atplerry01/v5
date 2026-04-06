namespace Whycespace.Shared.Contracts.Workflow;

/// <summary>
/// E18.6.2 — Workflow context for cross-SPV execution.
/// Carries transaction identity through the workflow pipeline.
/// </summary>
public sealed record CrossSpvWorkflowContext : IWorkflowContext
{
    public required Guid TransactionId { get; init; }
    public required Guid RootSpvId { get; init; }
    public required string CorrelationId { get; init; }

    // IWorkflowContext
    public string WorkflowId => "cluster.crossspv.execution";
    public string WorkflowType => "LONG_RUNNING";
    public string StepId { get; init; } = string.Empty;
    public string StepType { get; init; } = string.Empty;
    public string State { get; init; } = "pending";
    public string Transition { get; init; } = "start";
}
