namespace Whycespace.Domain.OrchestrationSystem.Workflow.Definition;

public sealed record DefinitionDraftedEvent(DefinitionId DefinitionId, WorkflowBlueprint Blueprint);
