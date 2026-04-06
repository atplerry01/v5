namespace Whyce.Runtime.Guards;

/// <summary>
/// Runtime isolation guard. Validates that the runtime boundary is not violated.
///
/// ENFORCED RULES:
/// - Runtime is the ONLY layer allowed to: Persist, Anchor to Chain, Publish to Kafka
/// - Engines MUST be stateless, NEVER persist, ONLY emit events
/// - Platform → Runtime (must go via systems, not direct)
/// - Systems → Engines (must go via runtime, not direct)
/// - No direct calls bypassing the control plane
/// </summary>
public static class RuntimeIsolationGuard
{
    /// <summary>
    /// Validates that a command execution originated from the runtime control plane.
    /// Returns false if the execution context does not have the runtime origin marker.
    /// </summary>
    public static bool ValidateRuntimeOrigin(bool runtimeOrigin)
    {
        return runtimeOrigin;
    }

    /// <summary>
    /// Validates that a policy decision has been made before engine execution.
    /// No engine execution without explicit policy approval.
    /// </summary>
    public static bool ValidatePolicyDecisionPresent(bool? policyDecisionAllowed, string? policyDecisionHash)
    {
        return policyDecisionAllowed is true && !string.IsNullOrWhiteSpace(policyDecisionHash);
    }

    /// <summary>
    /// Returns a structured violation report for isolation breaches.
    /// </summary>
    public static RuntimeIsolationViolation CreateViolation(string source, string target, string description)
    {
        return new RuntimeIsolationViolation(source, target, description);
    }
}

public sealed record RuntimeIsolationViolation(
    string Source,
    string Target,
    string Description);
