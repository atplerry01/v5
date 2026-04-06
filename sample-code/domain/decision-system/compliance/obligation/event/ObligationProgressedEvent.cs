namespace Whycespace.Domain.DecisionSystem.Compliance.Obligation;

using Whycespace.Domain.SharedKernel;

public sealed record ObligationProgressedEvent(Guid ObligationId) : DomainEvent;
