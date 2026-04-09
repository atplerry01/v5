using Whyce.Shared.Contracts.Engine;

namespace Whyce.Shared.Contracts.Runtime;

public interface IWorkflowStep
{
    string Name { get; }
    WorkflowStepType StepType { get; }

    // phase1.5-S5.2.3 / TC-7 (WORKFLOW-TIMEOUT-01): step contract now
    // consumes the per-step linked CancellationToken so a hung step
    // observes the declared per-step timeout (and the upstream
    // request/host token) at its first cancellable await. The token
    // is the linked CTS produced by T1MWorkflowEngine — it carries
    // both PerStepTimeoutMs expiry and execution-level / caller
    // cancellation.
    Task<WorkflowStepResult> ExecuteAsync(
        WorkflowExecutionContext context,
        CancellationToken cancellationToken = default);
}
