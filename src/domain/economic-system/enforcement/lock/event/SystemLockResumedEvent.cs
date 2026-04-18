using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Enforcement.Lock;

/// <summary>
/// Phase 7 T7.8 — lock resumed after a suspension window closed. The
/// original Lock-time cause, scope, reason, and ExpiresAt are restored
/// exactly — no aggregate state is invented.
/// </summary>
public sealed record SystemLockResumedEvent(
    LockId LockId,
    SubjectId SubjectId,
    Timestamp ResumedAt) : DomainEvent;
