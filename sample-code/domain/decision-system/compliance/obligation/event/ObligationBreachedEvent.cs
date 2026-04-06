namespace Whycespace.Domain.DecisionSystem.Compliance.Obligation;

using Whycespace.Domain.SharedKernel;

public sealed record ObligationBreachedEvent(
    Guid ObligationId,
    string Reason) : DomainEvent;
