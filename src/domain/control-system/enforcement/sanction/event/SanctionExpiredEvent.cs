using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Enforcement.Sanction;

public sealed record SanctionExpiredEvent(
    SanctionId SanctionId,
    SubjectId SubjectId,
    Timestamp ExpiredAt) : DomainEvent;
