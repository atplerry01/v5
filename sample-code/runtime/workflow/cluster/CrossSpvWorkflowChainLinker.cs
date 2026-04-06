using Whycespace.Runtime.Command;

namespace Whycespace.Runtime.Workflow.Cluster;

/// <summary>
/// E18.6.5 — Links cross-SPV workflow execution to policy decisions and chain evidence.
/// Attaches TransactionId and RootSpvId to command context for full traceability.
/// </summary>
public static class CrossSpvWorkflowChainLinker
{
    public const string TransactionIdKey = "CrossSpv.TransactionId";
    public const string RootSpvIdKey = "CrossSpv.RootSpvId";
    public const string WorkflowStepKey = "CrossSpv.WorkflowStep";

    public static void LinkCrossSpvContext(
        CommandContext context,
        CrossSpvWorkflowContext workflowContext,
        string stepId)
    {
        context.Set(TransactionIdKey, workflowContext.TransactionId.ToString());
        context.Set(RootSpvIdKey, workflowContext.RootSpvId.ToString());
        context.Set(WorkflowStepKey, stepId);

        // Also link standard workflow chain metadata
        WorkflowChainLinker.LinkDecision(
            context,
            decisionHash: string.Empty,
            workflowId: workflowContext.WorkflowId,
            stepId: stepId,
            state: workflowContext.State,
            transition: workflowContext.Transition);
    }
}
