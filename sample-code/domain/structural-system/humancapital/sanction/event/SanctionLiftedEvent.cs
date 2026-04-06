using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.HumanCapital.Sanction;

public sealed record SanctionLiftedEvent(
    Guid SanctionId
) : DomainEvent;
