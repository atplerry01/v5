namespace Whycespace.Shared.Contracts.Workflow;

/// <summary>
/// Workflow execution context for policy decision binding and chain anchoring (E7).
/// Resolved from command context by the runtime.
/// Carried through the pipeline to enrich policy decisions and chain evidence.
/// </summary>
public interface IWorkflowContext
{
    string WorkflowId { get; }
    string WorkflowType { get; }
    string StepId { get; }
    string StepType { get; }
    string State { get; }
    string Transition { get; }
}
