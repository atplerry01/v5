using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.ConstitutionalSystem.Policy.Rule;

public sealed record PolicyViolationDetectedEvent(
    Guid PolicyId,
    IReadOnlyList<string> ViolatedRules,
    IReadOnlyList<string> Reasons,
    string Severity,
    string DecisionHash,
    string CommandCorrelationId,
    DateTimeOffset DetectedAt) : DomainEvent;
