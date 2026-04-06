namespace Whycespace.Domain.DecisionSystem.Compliance.Regulation;

using Whycespace.Domain.SharedKernel;

public sealed record RegulationRevokedEvent(
    Guid RegulationId,
    string Reason) : DomainEvent;
