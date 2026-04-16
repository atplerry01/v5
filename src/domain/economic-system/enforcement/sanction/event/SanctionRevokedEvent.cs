using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Enforcement.Sanction;

public sealed record SanctionRevokedEvent(
    SanctionId SanctionId,
    SubjectId SubjectId,
    Reason RevocationReason,
    Timestamp RevokedAt) : DomainEvent;
