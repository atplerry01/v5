namespace Whycespace.Shared.Contracts.Economic.Enforcement.Workflow;

public static class EnforcementLifecycleWorkflowNames
{
    public const string Lifecycle = "economic.enforcement.lifecycle";
}

public static class EnforcementLifecycleSteps
{
    public const string EscalateViolation = "escalate_violation";
    public const string IssueSanction     = "issue_sanction";
    public const string ApplyRestriction  = "apply_restriction";
    public const string LockSystem        = "lock_system";
}
