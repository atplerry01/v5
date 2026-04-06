namespace Whycespace.Engines.T2E.Workflow.Transition;

public abstract record WorkflowTransitionCommand;

public sealed record CreateWorkflowTransitionCommand(string Id) : WorkflowTransitionCommand;
