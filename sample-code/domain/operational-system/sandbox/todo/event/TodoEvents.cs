namespace Whycespace.Domain.OperationalSystem.Sandbox.Todo;

public sealed record TodoCreatedEvent(
    Guid TodoId,
    string Title,
    string Description,
    string? AssignedTo,
    int Priority) : DomainEvent;

public sealed record TodoCompletedEvent(
    Guid TodoId) : DomainEvent;
