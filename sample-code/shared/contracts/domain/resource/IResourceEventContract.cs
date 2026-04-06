namespace Whycespace.Shared.Contracts.Domain.Resource;

/// <summary>
/// ACL boundary contract for resource domain events.
/// Consumers depend on this interface — never on domain entities directly.
/// </summary>
public interface IResourceEventContract
{
    Guid EventId { get; }
    string EventType { get; }
    DateTimeOffset OccurredAt { get; }
    int Version { get; }
    string CorrelationId { get; }

    Guid ResourceId { get; }
    string ResourceType { get; }
}
