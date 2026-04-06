namespace Whycespace.Shared.Contracts.Domain.Identity;

/// <summary>
/// ACL boundary contract for identity domain events.
/// Consumers depend on this interface — never on domain entities directly.
/// </summary>
public interface IIdentityEventContract
{
    Guid EventId { get; }
    string EventType { get; }
    DateTimeOffset OccurredAt { get; }
    int Version { get; }
    string CorrelationId { get; }

    Guid IdentityId { get; }
    string ActionType { get; }
    string? Reason { get; }
}