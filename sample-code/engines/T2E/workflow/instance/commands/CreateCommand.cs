namespace Whycespace.Engines.T2E.Workflow.Instance;

public abstract record WorkflowInstanceCommand;

public sealed record CreateWorkflowInstanceCommand(string Id) : WorkflowInstanceCommand;
