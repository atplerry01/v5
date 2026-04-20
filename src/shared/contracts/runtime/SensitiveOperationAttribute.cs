namespace Whycespace.Shared.Contracts.Runtime;

/// <summary>
/// R1 §7 — sensitive-operation marker. Commands decorated with this attribute
/// require elevated authorization beyond the standard pipeline: a distinct
/// policy scope (see <see cref="RequiredScope"/>) and a fresh/step-up session
/// context (when <see cref="RequiresStepUpAuthentication"/> is true).
///
/// <c>AuthorizationGuardMiddleware</c> (post-policy) consults this attribute
/// and rejects commands whose resolved identity lacks the required scope.
///
/// Examples of sensitive operations: system-lock commands, policy-override
/// commands, administrative actor-suspension commands, escalation override.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = true, AllowMultiple = false)]
public sealed class SensitiveOperationAttribute : Attribute
{
    /// <summary>Required policy scope (e.g. "administrative", "operator", "emergency"). Must match a declared WHYCEPOLICY scope.</summary>
    public string RequiredScope { get; }

    /// <summary>When true, the pipeline rejects unless the actor completed step-up / fresh authentication within the configured window.</summary>
    public bool RequiresStepUpAuthentication { get; init; }

    /// <summary>Human-readable rationale surfaced in audit events when the attribute causes a rejection.</summary>
    public string? Rationale { get; init; }

    public SensitiveOperationAttribute(string requiredScope)
    {
        if (string.IsNullOrWhiteSpace(requiredScope))
            throw new ArgumentException("RequiredScope must be non-empty.", nameof(requiredScope));
        RequiredScope = requiredScope;
    }
}
