namespace Whycespace.Domain.OrchestrationSystem.Workflow.Execution;

/// <summary>
/// Topic: whyce.orchestration.workflow.started
/// Command: WorkflowStartCommand
/// </summary>
public sealed record WorkflowStartedEvent(
    Guid WorkflowId,
    Guid DefinitionId,
    string WorkflowType) : DomainEvent;
