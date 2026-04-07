namespace Whycespace.Domain.OrchestrationSystem.Workflow.Execution;

public static class WorkflowExecutionErrors
{
    public const string NotRunning = "Workflow execution is not in Running state.";
    public const string WorkflowNameRequired = "Workflow name is required.";
    public const string CannotCompleteBeforeStarted = "Workflow execution cannot complete before it has started.";
    public const string CannotStepAfterCompleted = "Workflow execution cannot record a step after it has completed.";
    public const string CannotResumeUnlessFailed = "Workflow execution can only be resumed from the Failed state.";
    public const string CannotSkipSteps = "Workflow steps must be completed in order; the next expected step index does not match.";
    public const string StepNameRequired = "Step name is required.";
}
