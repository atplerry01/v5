namespace Whycespace.Shared.Contracts.Domain.Observability;

/// <summary>
/// ACL boundary contract for observability domain events.
/// Consumers depend on this interface — never on domain entities directly.
/// </summary>
public interface IObservabilityEventContract
{
    Guid EventId { get; }
    string EventType { get; }
    DateTimeOffset OccurredAt { get; }
    int Version { get; }
    string CorrelationId { get; }

    Guid SourceId { get; }
    string SourceType { get; }
    string Severity { get; }
}
