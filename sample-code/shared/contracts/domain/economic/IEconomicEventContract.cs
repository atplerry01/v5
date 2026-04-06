namespace Whycespace.Shared.Contracts.Domain.Economic;

/// <summary>
/// ACL boundary contract for economic domain events.
/// Consumers depend on this interface — never on domain entities directly.
/// </summary>
public interface IEconomicEventContract
{
    Guid EventId { get; }
    string EventType { get; }
    DateTimeOffset OccurredAt { get; }
    int Version { get; }
    string CorrelationId { get; }

    Guid SourceEntityId { get; }
    string SourceEntityType { get; }
    decimal Amount { get; }
    string CurrencyCode { get; }
    string Direction { get; }
}