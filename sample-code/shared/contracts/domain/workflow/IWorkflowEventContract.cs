namespace Whycespace.Shared.Contracts.Domain.Workflow;

/// <summary>
/// ACL boundary contract for workflow domain events.
/// Consumers depend on this interface — never on domain entities directly.
/// </summary>
public interface IWorkflowEventContract
{
    Guid EventId { get; }
    string EventType { get; }
    DateTimeOffset OccurredAt { get; }
    int Version { get; }
    string CorrelationId { get; }

    Guid WorkflowInstanceId { get; }
    string WorkflowType { get; }
    string CurrentState { get; }
    string? PreviousState { get; }
}
