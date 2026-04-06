namespace Whycespace.Shared.Contracts.Domain.Policy;

/// <summary>
/// ACL boundary contract for policy domain events.
/// Consumers depend on this interface — never on domain entities directly.
/// </summary>
public interface IPolicyEventContract
{
    Guid EventId { get; }
    string EventType { get; }
    DateTimeOffset OccurredAt { get; }
    int Version { get; }
    string CorrelationId { get; }

    Guid PolicyId { get; }
    string PolicyType { get; }
    string EvaluationResult { get; }
    string? ViolationCode { get; }
}
