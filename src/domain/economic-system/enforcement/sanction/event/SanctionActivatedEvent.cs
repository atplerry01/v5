using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Enforcement.Sanction;

public sealed record SanctionActivatedEvent(
    SanctionId SanctionId,
    SubjectId SubjectId,
    Timestamp ActivatedAt) : DomainEvent;
