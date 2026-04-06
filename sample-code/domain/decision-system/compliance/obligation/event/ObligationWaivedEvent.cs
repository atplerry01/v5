namespace Whycespace.Domain.DecisionSystem.Compliance.Obligation;

using Whycespace.Domain.SharedKernel;

public sealed record ObligationWaivedEvent(
    Guid ObligationId,
    string Reason) : DomainEvent;
