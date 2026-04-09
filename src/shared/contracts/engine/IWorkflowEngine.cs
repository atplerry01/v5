using Whyce.Shared.Contracts.Runtime;

namespace Whyce.Shared.Contracts.Engine;

public interface IWorkflowEngine
{
    // phase1.5-S5.2.3 / TC-7 (WORKFLOW-TIMEOUT-01): engine contract
    // now consumes the request/host-shutdown CancellationToken. The
    // engine internally builds an execution-level linked CTS bounded
    // by WorkflowOptions.MaxExecutionMs and a per-step linked CTS
    // bounded by WorkflowOptions.PerStepTimeoutMs. Caller-driven
    // cancellation propagates as OperationCanceledException; declared
    // timeouts surface as the typed WorkflowTimeoutException.
    Task<WorkflowExecutionResult> ExecuteAsync(
        WorkflowDefinition definition,
        WorkflowExecutionContext context,
        CancellationToken cancellationToken = default);
}
