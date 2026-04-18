namespace Whycespace.Domain.SharedKernel.Primitives.Kernel;

/// <summary>
/// Defense-in-depth enforcement check called at the top of every
/// restriction-gated command handler. The execution-guard middleware
/// hard-rejects restricted subjects before handlers are invoked, but
/// handlers still call this helper so:
///
///   1. Internal call sites that bypass the middleware (e.g. workflow
///      recovery code) cannot silently execute commands for a restricted
///      subject.
///   2. A misconfigured host that does not wire the middleware still
///      enforces restrictions at the engine boundary.
///
/// The Phase-2 contract locked restriction semantics as HARD REJECT.
/// The Phase-2.5 contract adds exactly one narrow bypass: commands
/// dispatched via <c>ISystemIntentDispatcher.DispatchSystemAsync</c>
/// carry <c>CommandContext.IsSystem = true</c>, which flows into the
/// engine context and causes this guard to pass. System-origin bypass
/// exists so workflow-driven compensation, settlement completion, and
/// recovery work can still complete when a subject becomes restricted
/// mid-workflow. There is no other bypass — no role flag, no header,
/// no "trusted caller" short-circuit.
///
/// Constraint format matches what ExecutionGuardMiddleware stamps on
/// CommandContext.EnforcementConstraint: "Restricted:{scope}" signals
/// an active restriction; anything else (null, "High", "Medium", …) is
/// a non-restriction constraint and the helper is a no-op.
/// </summary>
public static class EnforcementGuard
{
    public const string RestrictedPrefix = "Restricted:";

    public static void RequireNotRestricted(string? enforcementConstraint, bool isSystem)
    {
        // Phase 2.5 — the ONLY bypass surface. System-origin commands
        // (compensation, settlement completion, recovery) bypass the
        // restriction gate so workflow state can converge even when a
        // restriction is applied mid-flight.
        if (isSystem) return;

        if (string.IsNullOrEmpty(enforcementConstraint)) return;

        if (!enforcementConstraint.StartsWith(RestrictedPrefix, StringComparison.Ordinal)) return;

        var scope = enforcementConstraint.Substring(RestrictedPrefix.Length);
        throw new SubjectRestrictedException(scope);
    }
}
