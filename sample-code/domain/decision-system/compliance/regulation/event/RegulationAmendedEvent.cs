namespace Whycespace.Domain.DecisionSystem.Compliance.Regulation;

using Whycespace.Domain.SharedKernel;

public sealed record RegulationAmendedEvent(
    Guid RegulationId,
    string Description,
    int Major,
    int Minor,
    int Patch) : DomainEvent;
