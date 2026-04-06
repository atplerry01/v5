namespace Whycespace.Domain.ConstitutionalSystem.Policy.Jurisdiction;

/// <summary>
/// A jurisdiction-specific rule that overlays base policy.
/// Higher priority overrides lower priority in conflict resolution.
/// </summary>
public sealed record PolicyOverlayRule
{
    public string PolicyAction { get; }
    public OverlayEffect Effect { get; }
    public int Priority { get; }
    public string Justification { get; }

    private PolicyOverlayRule(string policyAction, OverlayEffect effect, int priority, string justification)
    {
        PolicyAction = policyAction;
        Effect = effect;
        Priority = priority;
        Justification = justification;
    }

    public static PolicyOverlayRule Create(string policyAction, OverlayEffect effect, int priority, string justification) =>
        string.IsNullOrWhiteSpace(policyAction)
            ? throw new ArgumentException("Policy action is required.")
            : new(policyAction, effect, priority, justification);
}

public enum OverlayEffect
{
    Allow,
    Deny,
    RequireAdditionalApproval,
    Restrict
}
