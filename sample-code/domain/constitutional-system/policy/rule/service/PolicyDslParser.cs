using System.Text.RegularExpressions;

namespace Whycespace.Domain.ConstitutionalSystem.Policy.Rule;

/// <summary>
/// Parses WhycePolicy DSL text into PolicyDefinition model.
/// DSL is declarative — no execution logic.
///
/// Example:
///   POLICY "vault.admin.access" PRIORITY 100
///   SCOPE global *
///   ALLOW IF actor.role == "admin" AND resource.type == "vault"
///   DENY IF capital.amount > limit
/// </summary>
public static partial class PolicyDslParser
{
    private static readonly Regex PolicyLine = PolicyLineRegex();
    private static readonly Regex ScopeLine = ScopeLineRegex();
    private static readonly Regex RuleLine = RuleLineRegex();
    private static readonly Regex ConstraintPattern = ConstraintPatternRegex();

    public static PolicyDefinition Parse(string dsl)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dsl);

        var lines = dsl
            .Split('\n')
            .Select(l => l.Trim())
            .Where(l => l.Length > 0 && !l.StartsWith('#'))
            .ToList();

        string name = "";
        string description = "";
        int priority = 0;
        ScopeDefinition? scope = null;
        var rules = new List<RuleDefinition>();

        foreach (var line in lines)
        {
            var policyMatch = PolicyLine.Match(line);
            if (policyMatch.Success)
            {
                name = policyMatch.Groups["name"].Value;
                description = name;
                if (policyMatch.Groups["priority"].Success)
                    priority = int.Parse(policyMatch.Groups["priority"].Value);
                continue;
            }

            var scopeMatch = ScopeLine.Match(line);
            if (scopeMatch.Success)
            {
                scope = new ScopeDefinition
                {
                    Type = scopeMatch.Groups["type"].Value.ToLowerInvariant(),
                    Target = scopeMatch.Groups["target"].Value
                };
                continue;
            }

            var ruleMatch = RuleLine.Match(line);
            if (ruleMatch.Success)
            {
                var decision = ruleMatch.Groups["decision"].Value.ToUpperInvariant();
                var conditionsText = ruleMatch.Groups["conditions"].Value;
                var constraints = ParseConstraints(conditionsText);

                rules.Add(new RuleDefinition
                {
                    Name = $"{name}.{decision.ToLowerInvariant()}.{rules.Count}",
                    Decision = decision,
                    Constraints = constraints
                });
            }
        }

        if (string.IsNullOrEmpty(name))
            throw new InvalidOperationException("DSL must contain a POLICY declaration.");

        return new PolicyDefinition
        {
            Name = name,
            Description = description,
            Scope = scope ?? ScopeDefinition.Global(),
            Priority = priority,
            Rules = rules
        };
    }

    private static IReadOnlyList<ConstraintDefinition> ParseConstraints(string conditionsText)
    {
        var parts = Regex.Split(conditionsText, @"\s+AND\s+", RegexOptions.IgnoreCase);
        var constraints = new List<ConstraintDefinition>();

        foreach (var part in parts)
        {
            var match = ConstraintPattern.Match(part.Trim());
            if (!match.Success) continue;

            constraints.Add(new ConstraintDefinition
            {
                Left = match.Groups["left"].Value,
                Operator = NormalizeOperator(match.Groups["op"].Value),
                Right = match.Groups["right"].Value.Trim('"')
            });
        }

        return constraints;
    }

    private static string NormalizeOperator(string op) => op switch
    {
        "==" => "eq",
        "!=" => "neq",
        ">" => "gt",
        ">=" => "gte",
        "<" => "lt",
        "<=" => "lte",
        _ => op.ToLowerInvariant()
    };

    [GeneratedRegex(@"^POLICY\s+""(?<name>[^""]+)""\s*(?:PRIORITY\s+(?<priority>\d+))?", RegexOptions.IgnoreCase)]
    private static partial Regex PolicyLineRegex();

    [GeneratedRegex(@"^SCOPE\s+(?<type>\w+)\s+(?<target>.+)$", RegexOptions.IgnoreCase)]
    private static partial Regex ScopeLineRegex();

    [GeneratedRegex(@"^(?<decision>ALLOW|DENY)\s+IF\s+(?<conditions>.+)$", RegexOptions.IgnoreCase)]
    private static partial Regex RuleLineRegex();

    [GeneratedRegex(@"(?<left>[\w.]+)\s*(?<op>==|!=|>=|<=|>|<|contains|in)\s*(?<right>""[^""]*""|\S+)")]
    private static partial Regex ConstraintPatternRegex();
}
