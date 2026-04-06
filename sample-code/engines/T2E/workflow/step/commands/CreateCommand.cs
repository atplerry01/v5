namespace Whycespace.Engines.T2E.Workflow.Step;

public abstract record WorkflowStepCommand;

public sealed record CreateWorkflowStepCommand(string Id) : WorkflowStepCommand;
