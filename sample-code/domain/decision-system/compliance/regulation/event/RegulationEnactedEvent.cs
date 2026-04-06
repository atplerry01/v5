namespace Whycespace.Domain.DecisionSystem.Compliance.Regulation;

using Whycespace.Domain.SharedKernel;

public sealed record RegulationEnactedEvent(
    Guid RegulationId,
    string Title,
    string Description,
    string TypeCode,
    int Major,
    int Minor,
    int Patch,
    Guid JurisdictionId) : DomainEvent;
