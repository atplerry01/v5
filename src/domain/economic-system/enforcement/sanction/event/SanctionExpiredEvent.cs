using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Enforcement.Sanction;

public sealed record SanctionExpiredEvent(
    SanctionId SanctionId,
    SubjectId SubjectId,
    Timestamp ExpiredAt) : DomainEvent;
