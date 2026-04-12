namespace Whycespace.Domain.ConstitutionalSystem.Policy.Rule;

public static class RuleErrors
{
    public static InvalidOperationException MissingId() =>
        new("RuleId is required and must not be empty.");

    public static InvalidOperationException MissingDefinition() =>
        new("Rule must include a valid definition.");

    public static InvalidOperationException InvalidStateTransition(RuleStatus current, string attemptedAction) =>
        new($"Cannot '{attemptedAction}' when current status is '{current}'.");
}
