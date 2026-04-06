namespace Whycespace.Domain.StructuralSystem.Entity.EntityRegistry;

public sealed record EntityBoundToWorkflowEvent(Guid EntityId, Guid WorkflowContextId) : DomainEvent;
