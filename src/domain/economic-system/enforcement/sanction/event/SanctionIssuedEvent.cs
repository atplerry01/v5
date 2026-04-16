using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Enforcement.Sanction;

public sealed record SanctionIssuedEvent(
    SanctionId SanctionId,
    SubjectId SubjectId,
    SanctionType Type,
    SanctionScope Scope,
    Reason Reason,
    EffectivePeriod Period,
    Timestamp IssuedAt) : DomainEvent;
