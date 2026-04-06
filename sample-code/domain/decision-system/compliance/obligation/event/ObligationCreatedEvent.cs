namespace Whycespace.Domain.DecisionSystem.Compliance.Obligation;

using Whycespace.Domain.SharedKernel;

public sealed record ObligationCreatedEvent(
    Guid ObligationId,
    Guid RegulationId,
    Guid JurisdictionId,
    string Title,
    string Description,
    DateTimeOffset DueDate,
    int GracePeriodDays) : DomainEvent;
