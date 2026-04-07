CLASSIFICATION: orchestration-system / workflow / resume
SOURCE: workflow eventification refactor (20260407-210000-runtime-workflow-state-eventification.md)
SEVERITY: S2 — MEDIUM (advisory, not blocking)

DESCRIPTION:
After the workflow eventification refactor, `WorkflowResumeCommand` is rejected
by `RuntimeCommandDispatcher` with a structured failure. The dispatcher cannot
implement resume directly because:
  1. Resume requires reconstructing `WorkflowExecutionAggregate` from the event
     store via `LoadFromHistory`.
  2. That requires referencing `Whycespace.Domain.OrchestrationSystem.Workflow.Execution.*`
     concrete types from `src/runtime/dispatcher/`.
  3. Runtime guard 11.R-DOM-01 forbids any `using Whycespace.Domain.*` in
     `src/runtime/**`.

PROPOSED_RULE / REMEDIATION:
Introduce `IWorkflowExecutionReplayService` in
`src/shared/contracts/runtime/` with the following surface:

    public interface IWorkflowExecutionReplayService
    {
        Task<WorkflowExecutionReplayState?> ReplayAsync(Guid workflowExecutionId);
    }

    public sealed record WorkflowExecutionReplayState(
        string WorkflowName,
        int CurrentStepIndex,
        string ExecutionHash,
        string Status);

Implement it under `src/engines/T1M/lifecycle/` (or a new
`src/platform/host/adapters/`) where domain references are permitted.
Inject the service into `RuntimeCommandDispatcher` and use it to populate
the resume `WorkflowExecutionContext`.

UNTIL THEN: callers of `WorkflowResumeCommand` must handle the structured
failure response.

TRACKED BY: runtime.guard.md R-WF-RESUME-01, runtime.audit.md CHECK R-WF-RESUME-01
