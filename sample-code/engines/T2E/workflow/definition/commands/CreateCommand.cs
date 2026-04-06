namespace Whycespace.Engines.T2E.Workflow.Definition;

public abstract record WorkflowDefinitionCommand;

public sealed record CreateWorkflowDefinitionCommand(string Id) : WorkflowDefinitionCommand;
