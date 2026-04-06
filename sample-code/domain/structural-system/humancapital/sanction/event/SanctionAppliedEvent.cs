using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.HumanCapital.Sanction;

public sealed record SanctionAppliedEvent(
    Guid SanctionId,
    string SanctionType,
    TimeSpan Duration
) : DomainEvent;
