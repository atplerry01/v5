using Whycespace.Runtime.Command;

namespace Whycespace.Runtime.Workflow;

/// <summary>
/// Links workflow step execution to policy decisions and chain evidence (E7).
/// Attaches DecisionHash and workflow metadata to the command context
/// so that each step execution is traceable through the chain.
/// </summary>
public static class WorkflowChainLinker
{
    public const string DecisionHashKey = "Workflow.DecisionHash";
    public const string WorkflowIdKey = "Workflow.WorkflowId";
    public const string StepIdKey = "Workflow.StepId";
    public const string StateKey = "Workflow.State";
    public const string TransitionKey = "Workflow.Transition";

    public static void LinkDecision(CommandContext context, string decisionHash,
        string workflowId, string stepId, string state, string transition)
    {
        context.Set(DecisionHashKey, decisionHash);
        context.Set(WorkflowIdKey, workflowId);
        context.Set(StepIdKey, stepId);
        context.Set(StateKey, state);
        context.Set(TransitionKey, transition);
    }

    public static string? GetLinkedDecisionHash(CommandContext context)
        => context.Get<string>(DecisionHashKey);
}
