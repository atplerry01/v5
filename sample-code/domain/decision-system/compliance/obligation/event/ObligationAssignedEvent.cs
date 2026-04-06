namespace Whycespace.Domain.DecisionSystem.Compliance.Obligation;

using Whycespace.Domain.SharedKernel;

public sealed record ObligationAssignedEvent(
    Guid ObligationId,
    Guid EntityId) : DomainEvent;
