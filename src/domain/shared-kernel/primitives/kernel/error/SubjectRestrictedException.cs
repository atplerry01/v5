namespace Whycespace.Domain.SharedKernel.Primitives.Kernel;

/// <summary>
/// Canonical hard-reject error raised by command handlers when the
/// executing subject has an active enforcement restriction. Inherits
/// from <see cref="DomainException"/> so the existing API middleware
/// (<c>DomainExceptionHandler</c>) maps it to HTTP 400 with the standard
/// problem+json payload — no bespoke serialization required.
///
/// Raised BEFORE any aggregate load or mutation so no events are emitted
/// and no workflow step progresses. Phase 2 locked decision: restrictions
/// are HARD REJECT, never soft-degrade.
/// </summary>
public sealed class SubjectRestrictedException : DomainException
{
    public string Scope { get; }
    public string? Reason { get; }

    public SubjectRestrictedException(string scope, string? reason = null)
        : base(BuildMessage(scope, reason))
    {
        Scope = scope;
        Reason = reason;
    }

    private static string BuildMessage(string scope, string? reason)
    {
        var suffix = string.IsNullOrWhiteSpace(reason) ? "" : $" Reason: {reason}";
        return $"Subject is restricted (scope={scope}). Command rejected.{suffix}";
    }
}
