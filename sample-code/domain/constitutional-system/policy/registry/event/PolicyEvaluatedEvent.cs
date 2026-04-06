using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.ConstitutionalSystem.Policy.Registry;

public sealed record PolicyEvaluatedEvent(
    Guid PolicyId,
    string DecisionType,
    Guid ActorId,
    string Action,
    string Resource,
    string Environment,
    DateTimeOffset EvaluatedAt) : DomainEvent;
