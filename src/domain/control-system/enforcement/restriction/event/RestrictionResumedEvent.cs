using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Enforcement.Restriction;

/// <summary>
/// Phase 7 T7.7 — restriction resumed after a suspension window closed
/// (e.g., the compensation flow that triggered suspension completed).
/// The original Apply-time cause, scope, and reason are restored exactly
/// — no aggregate state is invented.
/// </summary>
public sealed record RestrictionResumedEvent(
    RestrictionId RestrictionId,
    SubjectId SubjectId,
    Timestamp ResumedAt) : DomainEvent;
