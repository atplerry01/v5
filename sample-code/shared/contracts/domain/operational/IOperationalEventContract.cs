namespace Whycespace.Shared.Contracts.Domain.Operational;

/// <summary>
/// ACL boundary contract for operational domain events.
/// Consumers depend on this interface — never on domain entities directly.
/// </summary>
public interface IOperationalEventContract
{
    Guid EventId { get; }
    string EventType { get; }
    DateTimeOffset OccurredAt { get; }
    int Version { get; }
    string CorrelationId { get; }

    Guid SourceEntityId { get; }
    string SourceEntityType { get; }
    string Status { get; }
    Guid? AssignedOperatorId { get; }
}
