namespace Whycespace.Domain.ConstitutionalSystem.Policy.Rule;

public static class PolicyDslValidator
{
    private static readonly HashSet<string> ValidOperators =
        ["eq", "neq", "gt", "gte", "lt", "lte", "contains", "in"];

    private static readonly HashSet<string> ValidDecisions =
        ["ALLOW", "DENY"];

    private static readonly HashSet<string> ValidScopeTypes =
        ["global", "cluster", "entity", "jurisdiction"];

    public static DslValidationResult Validate(PolicyDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(definition.Name))
            errors.Add("Policy name is required.");

        if (definition.Priority < 0)
            errors.Add("Priority must be non-negative.");

        if (!ValidScopeTypes.Contains(definition.Scope.Type))
            errors.Add($"Invalid scope type: '{definition.Scope.Type}'.");

        if (definition.Rules.Count == 0)
            errors.Add("Policy must have at least one rule.");

        foreach (var rule in definition.Rules)
        {
            if (!ValidDecisions.Contains(rule.Decision))
                errors.Add($"Rule '{rule.Name}': invalid decision '{rule.Decision}'.");

            if (rule.Constraints.Count == 0)
                errors.Add($"Rule '{rule.Name}': must have at least one constraint.");

            foreach (var constraint in rule.Constraints)
            {
                if (!ValidOperators.Contains(constraint.Operator))
                    errors.Add($"Rule '{rule.Name}': invalid operator '{constraint.Operator}'.");

                if (string.IsNullOrWhiteSpace(constraint.Left))
                    errors.Add($"Rule '{rule.Name}': constraint left operand is empty.");

                if (string.IsNullOrWhiteSpace(constraint.Right))
                    errors.Add($"Rule '{rule.Name}': constraint right operand is empty.");
            }
        }

        return new DslValidationResult(errors.Count == 0, errors);
    }
}

public sealed record DslValidationResult(bool IsValid, IReadOnlyList<string> Errors);
