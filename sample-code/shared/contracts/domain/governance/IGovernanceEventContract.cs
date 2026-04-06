namespace Whycespace.Shared.Contracts.Domain.Governance;

/// <summary>
/// ACL boundary contract for governance domain events.
/// Consumers depend on this interface — never on domain entities directly.
/// </summary>
public interface IGovernanceEventContract
{
    Guid EventId { get; }
    string EventType { get; }
    DateTimeOffset OccurredAt { get; }
    int Version { get; }
    string CorrelationId { get; }

    Guid SubjectId { get; }
    string SubjectType { get; }
    string DecisionOutcome { get; }
    string? InitiatorId { get; }
}