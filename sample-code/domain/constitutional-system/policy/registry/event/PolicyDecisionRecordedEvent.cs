using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.ConstitutionalSystem.Policy.Rule;

public sealed record PolicyDecisionRecordedEvent(
    Guid PolicyId,
    string DecisionType,
    string DecisionHash,
    IReadOnlyList<Guid> PolicyIds,
    string? EvaluationTrace,
    string CommandCorrelationId,
    DateTimeOffset EvaluatedAt) : DomainEvent;
